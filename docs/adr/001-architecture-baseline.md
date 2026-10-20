# 001 - Architecture Baseline

**Date**: 2026-10-20
**Status**: Accepted

## Context

Building a Thai + international stock analysis web platform with Freemium model. Need to establish foundational architectural decisions before implementation begins.

## Decision

### Framework & Runtime
- **Backend**: .NET 10 (LTS, Nov 2025 - Nov 2027)
- **Frontend**: Next.js 15+ (App Router, RSC by default)
- **Database**: PostgreSQL 17 + Redis 7
- **Container**: Docker + Docker Compose, Nginx reverse proxy

### Data Providers
- **Primary**: Twelve Data (Free 800 req/day → Pro $79/mo → Ultra $999/mo)
  - Covers US, global EOD, fundamentals, crypto, forex, SET/mai (Pro+)
- **FX**: Frankfurter.dev (free, 105 central banks including BOT/THB)
- **Adapter Pattern**: `IMarketDataProvider` interface with factory by symbol prefix

### AI & Analysis
- **Model**: `gemini-2.5-flash` with billed API key (Tier 1+ from start)
- **Structured Output**: JSON Schema via `response_format` + `mime_type: application/json`
- **RAG**: Gemini File Search (managed) — no custom vector DB
- **Analysis Types**: `quick` (Free, 3/day) + `deep` (Premium, unlimited)

### Charting
- **Library**: Lightweight Charts (OSS, MIT, `npm i lightweight-charts`)
- **NOT**: TradingView Charting Library (private, paid, license negotiation)

### Auth & Security
- **Auth**: ASP.NET Core Identity + custom JWT (15m) + rotating refresh tokens (30d, bcrypt hash)
- **Rate Limiting**: Built-in `RateLimiter` middleware (tier-aware policies)
- **API Versioning**: `/api/v1/` path only, 90-day Sunset header

### ETL & Data Pipeline
- **Scheduler**: .NET `IHostedService` + `PeriodicTimer` (no Hangfire yet)
- **Schedule**: Daily 06:00 ICT (EOD), Hourly 09-16 ICT (intraday Premium), 5-min (FX)
- **Idempotency**: ULID `run_id` + unique index + dead-letter table
- **Backfill**: CLI command with token-bucket rate limiting + checkpoint resume

### Testing
- **Backend**: xUnit + FluentAssertions + Testcontainers (Postgres/Redis) + WireMock (providers) + snapshot (AI schema)
- **Frontend**: Playwright E2E only + Aria snapshot for charts
- **Contract**: Pact (consumer-driven) with Pact Broker
- **Load**: k6 (100 VUs, p95 < 500ms, error < 1%)

### Observability
- **OpenTelemetry** → Grafana Cloud (free) + Sentry (free)

### Freemium Tiers
| Feature | Free | Premium |
|---------|------|---------|
| Watchlists | 1 (20 symbols) | Unlimited |
| Charts | 15-min delayed | Real-time |
| AI Analysis | 3 quick/day | Unlimited quick + deep |
| Alerts | — | 50 (email + webhook) |
| Data | EOD only | Intraday 1m/5m/15m/1h |

## Consequences

### Positive
- Single provider (Twelve Data) covers both Thai and international markets
- Managed RAG eliminates vector DB infrastructure
- OSS charting library avoids licensing costs
- .NET 10 LTS gives 2 years support
- Stdlib-first ETL scheduler reduces dependencies

### Negative
- Twelve Data Pro required for SET/mai ($229/mo)
- Free tier limits may frustrate early users
- No real-time data for Free tier (15-min delay)
- Gemini billing required from day 1

## Alternatives Considered

| Option | Rejected Because |
|--------|------------------|
| Alpha Vantage | Lower market coverage, separate data entitlement for real-time |
| Polygon.io (now Massive) | Pricing opaque, rebranded |
| TradingView Charting Library | Private, paid, no public pricing |
| Custom RAG + pgvector | Extra infra, Gemini File Search is managed |
| Hangfire | Overkill for simple cron; `PeriodicTimer` is zero-dep |
| IdentityServer/Duende | Overkill for single API |

## Related

- ADR 002: Database Schema Design (pending)
- ADR 003: API Contract & JSON Schemas (pending)
- ADR 004: Frontend Architecture & State Management (pending)