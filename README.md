# Stock Analysis Platform

Thai + international stock analysis web platform with hybrid API architecture, AI-powered analysis, and real-time market data.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Stock Analysis Platform                   │
├─────────────────────────────────────────────────────────────────┤
│  Frontend (Next.js 16)          │  Backend (.NET 10)            │
│  ┌─────────────────────────┐    │  ┌─────────────────────────┐  │
│  │ Dashboard (shadcn/ui)   │    │  │ API (ASP.NET Core)      │  │
│  │ Watchlist Management    │◄───┤  │ Auth (JWT + Refresh)    │  │
│  │ Charts (Lightweight)    │    │  │ Watchlist CRUD          │  │
│  │ AI Analysis Display     │    │  │ Market Data Endpoints   │  │
│  └─────────────────────────┘    │  │ AI Analysis Endpoints   │  │
│                                 │  └───────────┬─────────────┘  │
└─────────────────────────────────┼──────────────┼────────────────┘
                                  │              │
                    ┌─────────────┴──────────────┴─────────────┐
                    │           Infrastructure                  │
                    │  ┌─────────────┐  ┌─────────────────────┐ │
                    │  │ PostgreSQL 17│  │ Redis 7             │ │
                    │  │ (Timescale)  │  │ (Caching/Realtime)  │ │
                    │  └─────────────┘  └─────────────────────┘ │
                    │  ┌─────────────────────────────────────┐  │
                    │  │ ETL Worker (BackgroundService)      │  │
                    │  │ TwelveData → Normalize → Upsert     │  │
                    │  └─────────────────────────────────────┘  │
                    └────────────────────────────────────────────┘
```

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | .NET 10, ASP.NET Core, EF Core 9 |
| **Database** | PostgreSQL 17 (TimescaleDB), Redis 7 |
| **Frontend** | Next.js 16, React 19, Tailwind CSS v4, shadcn/ui |
| **Charts** | Lightweight Charts (TradingView OSS) |
| **Auth** | JWT (15m) + Refresh Token Rotation (30d), Bcrypt |
| **AI** | Google Gemini 2.5 Flash (Structured JSON Output) |
| **Market Data** | Twelve Data API (Adapter Pattern) |
| **Testing** | xUnit, Playwright, Pact |
| **CI/CD** | GitHub Actions, Docker, Nginx |

## Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker & Docker Compose
- PostgreSQL 17 (via Docker)
- Redis 7 (via Docker)
- Twelve Data API Key
- Google Gemini API Key

## Quick Start

### 1. Clone and Configure

```bash
git clone https://github.com/supakorn-chaisri/Stock-Analysis-Platform.git
cd Stock-Analysis-Platform

# Copy environment template
cp .env.example .env

# Edit .env with your API keys
# Required: TWELVE_DATA_API_KEY, GEMINI_API_KEY, JWT_KEY, POSTGRES_PASSWORD
```

### 2. Start Infrastructure

```bash
# Start PostgreSQL and Redis
docker compose up -d postgres redis

# Verify services are healthy
docker compose ps
```

### 3. Run Database Migrations

```bash
cd src/Api
dotnet ef database update
```

### 4. Start Backend API

```bash
cd src/Api
dotnet run

# API available at http://localhost:5000
# Swagger UI at http://localhost:5000/swagger
```

### 5. Start Frontend

```bash
cd web
npm install
npm run dev

# Frontend available at http://localhost:3000
```

### 6. Start ETL Worker (Optional)

```bash
cd src/Etl
dotnet run

# Fetches market data every hour
```

## Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `POSTGRES_PASSWORD` | PostgreSQL password | Yes |
| `JWT_KEY` | JWT signing key (min 32 chars) | Yes |
| `TWELVE_DATA_API_KEY` | Twelve Data API key | Yes |
| `GEMINI_API_KEY` | Google Gemini API key | Yes |
| `SENDGRID_API_KEY` | SendGrid for email alerts | No |
| `SENTRY_DSN` | Sentry error tracking | No |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry endpoint | No |

## API Endpoints

### Authentication
```
POST   /api/v1/auth/register     # Register new user
POST   /api/v1/auth/login        # Login (returns access + refresh tokens)
POST   /api/v1/auth/refresh      # Refresh access token
POST   /api/v1/auth/logout       # Revoke refresh token
```

### Watchlists
```
GET    /api/v1/watchlists              # List user's watchlists
GET    /api/v1/watchlists/{id}         # Get watchlist with symbols
POST   /api/v1/watchlists              # Create watchlist
PUT    /api/v1/watchlists/{id}         # Update watchlist name
DELETE /api/v1/watchlists/{id}         # Delete watchlist
POST   /api/v1/watchlists/{id}/symbols # Add symbol
DELETE /api/v1/watchlists/{id}/symbols/{symbol} # Remove symbol
PUT    /api/v1/watchlists/{id}/symbols # Reorder symbols
```

### Market Data
```
GET /api/v1/market-data/quote/{symbol}              # Real-time quote
GET /api/v1/market-data/bars/daily/{symbol}         # Daily bars (EOD)
GET /api/v1/market-data/bars/intraday/{symbol}      # Intraday bars
GET /api/v1/market-data/fundamentals/{symbol}       # Financial fundamentals
GET /api/v1/market-data/corporate-actions/{symbol}  # Splits, dividends
```

### AI Analysis (Premium)
```
POST /api/v1/ai-analysis              # Request analysis
GET  /api/v1/ai-analysis/{symbol}     # Get cached analysis
```

## Database Schema

Key tables (see `init-scripts/01-schema.sql`):

- **users** - ASP.NET Core Identity + tier (free/premium)
- **watchlists** - User watchlists with ordering
- **daily_bars** - EOD OHLCV data (BRIN indexed)
- **intraday_bars** - Intraday OHLCV (1m, 5m, 15m, 1h)
- **fundamentals** - Normalized financial metrics
- **corporate_actions** - Splits, dividends, mergers
- **ai_analyses** - Cached AI responses (JSONB)
- **alerts** - Price/volume alerts with channels
- **etl_runs** - ETL job tracking with ULID
- **etl_dead_letter** - Failed record queue

## Development

### Running Tests

```bash
# Backend tests
dotnet test tests/Api.Tests/StockAnalysis.Api.Tests.csproj
dotnet test tests/Etl.Tests/StockAnalysis.Etl.Tests.csproj

# Frontend tests
cd web
npm run test          # Playwright E2E
npm run test:pact     # Pact contract tests
```

### Code Quality

```bash
# Backend
dotnet build StockAnalysis.slnx
dotnet format --verify-no-changes

# Frontend
cd web
npm run lint
npx tsc --noEmit
```

### Adding a New Market Data Provider

1. Implement `IMarketDataProvider` in `src/Infrastructure/MarketData/`
2. Register in `DependencyInjection.cs`
3. Add configuration in `appsettings.json`

```csharp
public class MyProvider : IMarketDataProvider
{
    public string Name => "MyProvider";
    
    public async Task<MarketDataQuote?> GetQuoteAsync(Symbol symbol, CancellationToken ct = default) { ... }
    public async Task<IReadOnlyList<DailyBar>> GetDailyBarsAsync(Symbol symbol, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default) { ... }
    // ... other methods
}
```

## Production Deployment

### Docker Compose (Production)

```bash
# Build and start all services
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Services:
# - postgres:5432
# - redis:6379
# - api:8080 (internal)
# - web:3000 (internal)
# - nginx:80/443 (reverse proxy + SSL)
# - certbot: ACME challenge
```

### Nginx Configuration

- Reverse proxy to API (port 8080) and Web (port 3000)
- SSL termination via Let's Encrypt (certbot)
- Rate limiting, security headers

### Health Checks

```bash
# API health
curl http://localhost:5000/api/v1/health

# Database
docker exec postgres pg_isready -U postgres

# Redis
docker exec redis redis-cli ping
```

## Project Structure

```
Stock-Analysis-Platform/
├── .github/workflows/ci.yml          # CI/CD pipeline
├── docs/
│   ├── adr/                          # Architecture Decision Records
│   └── agents/                       # Agent configuration
├── init-scripts/
│   └── 01-schema.sql                 # Database schema
├── nginx/                            # Nginx configs
├── src/
│   ├── Api/                          # ASP.NET Core Web API
│   │   ├── Auth/                     # JWT, tokens, endpoints
│   │   ├── Endpoints/                # Minimal API endpoints
│   │   └── Middleware/               # Auth, rate limiting
│   ├── Domain/                       # Domain layer (no dependencies)
│   │   ├── Entities/                 # Domain entities
│   │   ├── Enums/                    # Domain enums
│   │   ├── Interfaces/               # Repository/service interfaces
│   │   └── ValueObjects/             # Symbol, Price, Volume
│   ├── Etl/                          # ETL Background Worker
│   │   └── Jobs/                     # MarketDataEtlJob
│   └── Infrastructure/               # Infrastructure layer
│       ├── MarketData/               # Provider implementations
│       ├── Persistence/              # EF Core, configurations
│       └── Repositories/             # Repository implementations
├── tests/
│   ├── Api.Tests/                    # xUnit + Testcontainers
│   └── Etl.Tests/                    # xUnit + WireMock
├── web/                              # Next.js Frontend
│   ├── src/app/                      # App Router pages
│   ├── tests/
│   │   ├── e2e/                      # Playwright tests
│   │   └── pact/                     # Pact consumer tests
│   └── playwright.config.ts
├── docker-compose.yml                # Development
├── docker-compose.prod.yml           # Production
├── .env.example                      # Environment template
└── StockAnalysis.slnx                # Solution file
```

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/ci.yml`):

1. **Backend Tests** - PostgreSQL 17 + Redis 7 services, xUnit, coverage
2. **Frontend Tests** - Lint, TypeScript, Build, Playwright E2E
3. **Contract Tests** - Pact consumer (frontend) + provider (backend)
4. **Docker Build** - Multi-stage builds for API and Web
5. **Deploy** - SSH to VPS, docker compose pull + up

## Freemium Model

| Feature | Free | Premium |
|---------|------|---------|
| Personal Watchlist | ✓ | ✓ |
| Basic Charts | ✓ | ✓ |
| AI Quick Summary | ✓ | ✓ |
| AI Deep Analysis | ✗ | ✓ |
| Custom Screener | ✗ | ✓ |
| Price Alerts (Email/Webhook) | ✗ | ✓ |

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## License

MIT License - see LICENSE file for details.

## Support

- Issues: [GitHub Issues](https://github.com/supakorn-chaisri/Stock-Analysis-Platform/issues)
- Documentation: [docs/](docs/)