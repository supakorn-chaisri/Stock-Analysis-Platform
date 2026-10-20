# Glossary

Canonical domain vocabulary for the Stock Analysis Platform. All code, docs, and communication must use these terms exactly.

---

## Core Concepts

| Term | Definition | Example |
|------|------------|---------|
| **Symbol** | Canonical identifier for a tradeable instrument. Format: `{EXCHANGE}:{TICKER}`. Case-sensitive. | `SET:PTT`, `NASDAQ:AAPL`, `BKK:PTT`, `FX:USDTHB` |
| **Exchange** | Market venue where the symbol trades. Uses standard MIC codes where possible. | `SET`, `NASDAQ`, `NYSE`, `BKK`, `FX` |
| **Ticker** | Exchange-specific symbol code. | `PTT`, `AAPL`, `USDTHB` |
| **Timeframe** | Bar granularity. One of: `1m`, `5m`, `15m`, `1h`, `1d`. | `1d` = daily/EOD; `1m` = 1-minute intraday |
| **Bar** / **Candle** | Single OHLCV record for a symbol at a specific timeframe and timestamp. | `{o: 45.50, h: 46.00, l: 45.00, c: 45.75, v: 1000000}` |
| **Adjusted Close** | Close price adjusted for all corporate actions (splits, dividends) up to that date. Used for returns calculations. | Raw close: 100 → 2:1 split → adjusted close: 50 |
| **Corporate Action** | Company-initiated event affecting share structure or value: split, dividend, rights issue, merger. | `2:1 split`, `$0.50 dividend` |
| **Fundamental Metric** | Normalized financial statement line item (revenue, net income, EPS, total assets, etc.) for a specific period. | `revenue: 1.2B`, `eps_diluted: 2.45` |
| **Period Type** | Reporting period: `quarterly` or `annual`. | `quarterly` = Q1/Q2/Q3/Q4; `annual` = FY |

---

## User & Access

| Term | Definition |
|------|------------|
| **Tier** | User subscription level: `free` or `premium`. |
| **Watchlist** | User-defined named list of symbols. Free: 1 watchlist, max 20 symbols. Premium: unlimited. |
| **Alert** | User-defined condition on a symbol that triggers notification via configured channels. |
| **Alert Condition** | One of: `price_above`, `price_below`, `pct_change`, `volume_spike`. |
| **Alert Channel** | Delivery method: `email` or `webhook`. |
| **Idempotency Key** | Client-generated UUID v4 sent in `Idempotency-Key` header to safely retry mutating requests. |

---

## AI & Analysis

| Term | Definition |
|------|------------|
| **Analysis Type** | `quick` (Free tier, 3/day) or `deep` (Premium, unlimited). |
| **Horizon** | Analysis lookahead window: `1d`, `1w`, `1m`. |
| **Structured Output** | AI response conforming to a predefined JSON Schema (verdict, score, key_drivers[], risks[], price_targets{}, confidence). |
| **File Search** | Gemini managed RAG tool (replaces custom vector search). |
| **Request Hash** | SHA256 of analysis request parameters used for caching/idempotency. |

---

## Data Pipeline

| Term | Definition |
|------|------------|
| **ETL Run** | Single execution of a data ingestion job. Identified by ULID `run_id`. |
| **Provider Adapter** | Implementation of `IMarketDataProvider` for a specific data source (Twelve Data, Frankfurter). |
| **Dead Letter** | Failed ETL record stored for inspection and retry. |
| **Checkpoint** | Last successfully processed date/symbol for resumable backfill. |
| **Token Bucket** | Per-provider rate limiter to respect API quotas. |

---

## Technical

| Term | Definition |
|------|------------|
| **BRIN Index** | Block Range Index — compact index for append-only time-series data (10x smaller than B-tree). |
| **ULID** | Universally Unique Lexicographically Sortable Identifier (26 chars, sortable, used for `run_id`). |
| **RSC** | React Server Component (Next.js App Router default). |
| **SWR** | Stale-While-Revalidate data fetching library (frontend). |
| **SSE** | Server-Sent Events (used for streaming AI analysis). |
| **OTLP** | OpenTelemetry Protocol (traces/metrics/logs export). |

---

## Enums (must match database)

```csharp
// Timeframe
public enum Timeframe { M1, M5, M15, H1, D1 }

// AnalysisType
public enum AnalysisType { Quick, Deep }

// AlertCondition
public enum AlertCondition { PriceAbove, PriceBelow, PctChange, VolumeSpike }

// AlertChannel
public enum AlertChannel { Email, Webhook }

// UserTier
public enum UserTier { Free, Premium }
```

---

## Naming Conventions

| Artifact | Convention | Example |
|----------|------------|---------|
| Database tables | snake_case, plural | `daily_bars`, `watchlist_symbols` |
| Database columns | snake_case | `adjusted_close`, `created_at` |
| C# classes/properties | PascalCase | `DailyBar`, `AdjustedClose` |
| C# private fields | _camelCase | `_connectionString` |
| TypeScript interfaces | PascalCase | `DailyBar`, `Watchlist` |
| TypeScript variables | camelCase | `dailyBars`, `watchlist` |
| API paths | kebab-case, plural | `/api/v1/watchlists`, `/api/v1/analysis` |
| JSON keys | camelCase | `adjustedClose`, `createdAt` |
| Environment variables | SCREAMING_SNAKE_CASE | `POSTGRES_PASSWORD`, `JWT_KEY` |

---

## Forbidden / Deprecated Terms

| Don't Use | Use Instead |
|-----------|-------------|
| `stock` | `symbol` or `instrument` |
| `candle` (in code) | `bar` |
| `timeframe` as string in C# | `Timeframe` enum |
| `adjustedClose` (snake_case in JSON) | `adjustedClose` (camelCase) |
| `user_id` in C# | `UserId` |
| `free`/`premium` as strings | `UserTier` enum |