-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "btree_gin";

-- Create enum types
CREATE TYPE timeframe AS ENUM ('1m', '5m', '15m', '1h', '1d');
CREATE TYPE analysis_type AS ENUM ('quick', 'deep');
CREATE TYPE alert_condition AS ENUM ('price_above', 'price_below', 'pct_change', 'volume_spike');
CREATE TYPE alert_channel AS ENUM ('email', 'webhook');
CREATE TYPE user_tier AS ENUM ('free', 'premium');

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    tier user_tier NOT NULL DEFAULT 'free',
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Refresh tokens (rotating)
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL,
    parent_token_hash VARCHAR(255), -- for rotation chain detection
    expires_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token_hash ON refresh_tokens(token_hash);

-- Watchlists
CREATE TABLE watchlists (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(100) NOT NULL,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_watchlists_user_id ON watchlists(user_id);

-- Watchlist symbols (many-to-many with ordering)
CREATE TABLE watchlist_symbols (
    watchlist_id UUID NOT NULL REFERENCES watchlists(id) ON DELETE CASCADE,
    symbol VARCHAR(50) NOT NULL, -- format: EXCHANGE:TICKER
    sort_order INT NOT NULL DEFAULT 0,
    added_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (watchlist_id, symbol)
);

-- Daily bars (EOD)
CREATE TABLE daily_bars (
    symbol VARCHAR(50) NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL,
    open NUMERIC(18,6) NOT NULL,
    high NUMERIC(18,6) NOT NULL,
    low NUMERIC(18,6) NOT NULL,
    close NUMERIC(18,6) NOT NULL,
    adjusted_close NUMERIC(18,6) NOT NULL,
    volume BIGINT NOT NULL,
    PRIMARY KEY (symbol, timestamp)
);

-- Intraday bars
CREATE TABLE intraday_bars (
    symbol VARCHAR(50) NOT NULL,
    timeframe timeframe NOT NULL,
    timestamp TIMESTAMPTZ NOT NULL,
    open NUMERIC(18,6) NOT NULL,
    high NUMERIC(18,6) NOT NULL,
    low NUMERIC(18,6) NOT NULL,
    close NUMERIC(18,6) NOT NULL,
    volume BIGINT NOT NULL,
    PRIMARY KEY (symbol, timeframe, timestamp)
);

-- BRIN indexes for time-series (much smaller than B-tree for append-only)
CREATE INDEX idx_daily_bars_timestamp_brin ON daily_bars USING BRIN (timestamp);
CREATE INDEX idx_intraday_bars_timestamp_brin ON intraday_bars USING BRIN (timestamp);
CREATE INDEX idx_daily_bars_symbol_timestamp ON daily_bars (symbol, timestamp DESC);
CREATE INDEX idx_intraday_bars_symbol_timeframe_timestamp ON intraday_bars (symbol, timeframe, timestamp DESC);

-- Fundamentals (normalized, not JSON)
CREATE TABLE fundamentals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol VARCHAR(50) NOT NULL,
    period_end DATE NOT NULL,
    period_type VARCHAR(20) NOT NULL, -- 'quarterly' or 'annual'
    metric VARCHAR(100) NOT NULL, -- e.g., 'revenue', 'net_income', 'total_assets', 'eps_diluted'
    value NUMERIC(24,6) NOT NULL,
    unit VARCHAR(20) NOT NULL DEFAULT 'native', -- 'native', 'millions', 'billions'
    source VARCHAR(50) NOT NULL, -- 'twelve_data', 'manual'
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (symbol, period_end, period_type, metric)
);

CREATE INDEX idx_fundamentals_symbol_period ON fundamentals (symbol, period_end DESC, period_type);

-- Corporate actions
CREATE TABLE corporate_actions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol VARCHAR(50) NOT NULL,
    action_type VARCHAR(20) NOT NULL, -- 'split', 'dividend', 'rights', 'merger'
    effective_date DATE NOT NULL,
    ratio NUMERIC(10,6), -- for splits: new/old (e.g., 2.0 for 2:1)
    cash_amount NUMERIC(18,6), -- for dividends
    currency VARCHAR(3) DEFAULT 'USD',
    description TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_corporate_actions_symbol_date ON corporate_actions (symbol, effective_date DESC);

-- AI Analysis cache
CREATE TABLE ai_analyses (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    symbol VARCHAR(50) NOT NULL,
    analysis_type analysis_type NOT NULL,
    horizon VARCHAR(10) NOT NULL, -- '1d', '1w', '1m'
    request_hash VARCHAR(64) NOT NULL, -- SHA256 of request params for idempotency
    response_json JSONB NOT NULL, -- structured response per schema
    model_version VARCHAR(50) NOT NULL, -- e.g., 'gemini-2.5-flash-001'
    tokens_used INT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_ai_analyses_symbol_type ON ai_analyses (symbol, analysis_type, horizon);
CREATE INDEX idx_ai_analyses_request_hash ON ai_analyses (request_hash);

-- Alerts
CREATE TABLE alerts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    symbol VARCHAR(50) NOT NULL,
    condition alert_condition NOT NULL,
    threshold NUMERIC(18,6) NOT NULL,
    channels alert_channel[] NOT NULL DEFAULT ARRAY['email'],
    webhook_url VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_triggered_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_alerts_user_active ON alerts (user_id, is_active);
CREATE INDEX idx_alerts_symbol_active ON alerts (symbol, is_active);

-- Alert triggers (audit log)
CREATE TABLE alert_triggers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    alert_id UUID NOT NULL REFERENCES alerts(id) ON DELETE CASCADE,
    triggered_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    current_value NUMERIC(18,6) NOT NULL,
    channel alert_channel NOT NULL,
    delivered BOOLEAN NOT NULL DEFAULT FALSE,
    error_message TEXT
);

CREATE INDEX idx_alert_triggers_alert_id ON alert_triggers (alert_id, triggered_at DESC);

-- Feature flags
CREATE TABLE feature_flags (
    key VARCHAR(100) PRIMARY KEY,
    enabled BOOLEAN NOT NULL DEFAULT FALSE,
    rollout_pct INT NOT NULL DEFAULT 100, -- 0-100
    tier_restriction user_tier, -- NULL = all tiers
    description TEXT,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ETL run tracking
CREATE TABLE etl_runs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    run_id VARCHAR(26) NOT NULL UNIQUE, -- ULID
    job_name VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL, -- 'running', 'completed', 'failed', 'partial'
    symbols_processed INT NOT NULL DEFAULT 0,
    records_inserted INT NOT NULL DEFAULT 0,
    records_failed INT NOT NULL DEFAULT 0,
    started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    error_message TEXT
);

CREATE INDEX idx_etl_runs_job_name ON etl_runs (job_name, started_at DESC);

-- ETL dead letter queue
CREATE TABLE etl_dead_letter (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    run_id VARCHAR(26) NOT NULL REFERENCES etl_runs(run_id),
    symbol VARCHAR(50) NOT NULL,
    timeframe timeframe,
    payload JSONB NOT NULL,
    error_message TEXT NOT NULL,
    retry_count INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at TIMESTAMPTZ
);

CREATE INDEX idx_etl_dead_letter_run_id ON etl_dead_letter (run_id);

-- User preferences (JSONB for flexible widget layout)
CREATE TABLE user_preferences (
    user_id UUID PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    dashboard_layout JSONB NOT NULL DEFAULT '[]'::jsonb, -- react-grid-layout format
    theme VARCHAR(20) NOT NULL DEFAULT 'system', -- 'light', 'dark', 'system'
    timezone VARCHAR(50) NOT NULL DEFAULT 'UTC',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Idempotency keys (24h TTL)
CREATE TABLE idempotency_keys (
    key VARCHAR(64) PRIMARY KEY, -- UUID v4
    response_json JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX idx_idempotency_keys_expires ON idempotency_keys (expires_at);