# MyPost

MyPost is a portfolio-ready virtual postal platform for creating, operating, and tracking shipments. It combines a role-aware React experience with a secure ASP.NET Core API, explicit shipment lifecycle rules, PostgreSQL persistence, and an optional CLI adapter.

> MyPost is a demonstration product. Tracking events, couriers, and seeded records are simulated and do not represent a real postal carrier or live vehicle location.

## Product capabilities

- Public, privacy-safe tracking by code
- Customer registration, address book, guided shipment creation, history, and cancellation
- Courier queues, shipment detail, status transitions, and delivery outcomes
- Administration overview, filters, assignment, lifecycle controls, user directory, and analytics
- Explicit, idempotent lifecycle rules with immutable address snapshots and tracking history
- JWT authentication, rotating HttpOnly refresh cookies, ASP.NET Core Identity, rate limiting, CORS, and Problem Details
- Responsive light/dark interface at 375, 768, 1024, and 1440px

## Architecture

MyPost is a modular monolith whose dependencies point inward:

```text
React Web ───────────────┐
ASP.NET Core API ────────┼──> Application ──> Domain
CLI adapter ─────────────┘          │
                                    └── ports <── Infrastructure <── PostgreSQL
                                                   └── Identity / JWT
```

| Project | Responsibility |
|---|---|
| `src/MyPost.Domain` | Aggregates, value objects, lifecycle invariants, and domain enums |
| `src/MyPost.Application` | Authorized use cases, DTOs, pricing, pagination, and persistence ports |
| `src/MyPost.Infrastructure` | EF Core/Npgsql, Identity, JWT, refresh tokens, migrations, and development seed |
| `apps/MyPost.Api` | Versioned Minimal API, policies, rate limits, OpenAPI, health checks, and error mapping |
| `apps/MyPost.Web` | React role experiences, remote state, forms, responsive UI, and accessibility states |
| `apps/MyPost.Cli` | Thin public-tracking adapter over the shared application layer |
| `tests/*` | Domain, application, and API integration coverage |

Architecture details live in [docs/architecture/overview.md](docs/architecture/overview.md), lifecycle rules in [docs/architecture/shipment-lifecycle.md](docs/architecture/shipment-lifecycle.md), and API behavior in [docs/api/conventions.md](docs/api/conventions.md).

## Technology

- .NET 10, ASP.NET Core Minimal APIs, EF Core 10, Npgsql, PostgreSQL 17
- ASP.NET Core Identity, JWT bearer authentication, rotating refresh tokens
- React 19, TypeScript 6 strict mode, Vite 8, Tailwind CSS 4
- TanStack Query, React Hook Form, Zod, Radix UI, Lucide, Recharts
- MSTest, Vitest, Testing Library, Playwright
- Docker Compose, multi-stage images, Nginx

## Quick start with Docker

Prerequisites: Docker Desktop with the Linux engine running.

```powershell
Copy-Item .env.example .env
docker compose up --build
```

Open:

- Web: `http://localhost:5173`
- API health: `http://localhost:8080/health`
- Swagger UI: `http://localhost:8080/swagger`

The Compose environment applies the migration and seeds demo records on first start. Values in `.env.example` are local-development defaults; replace them before sharing or deploying an environment.

Stop containers with `docker compose down`. Add `--volumes` only when you intentionally want to remove the local PostgreSQL data volume.

## Native development

Prerequisites: .NET SDK 10, Node.js 24+, npm, and PostgreSQL 17.

Create the database, then configure API secrets without committing them:

```powershell
$env:ConnectionStrings__MyPost = 'Host=localhost;Port=5432;Database=mypost;Username=mypost;Password=choose-a-password'
$env:Jwt__SigningKey = 'choose-a-long-random-signing-key-at-least-32-characters'
$env:Seed__Password = 'choose-a-development-demo-password'
dotnet run --project apps/MyPost.Api
```

The development profile listens on `http://localhost:8080`, automatically applies migrations, and enables the opt-in seed only when `Seed__Password` is set.

In another terminal:

```powershell
Set-Location apps/MyPost.Web
npm ci
npm run dev
```

The web app opens at `http://localhost:5173` and targets `http://localhost:8080/api/v1` by default. Override it with `VITE_API_URL` when needed.

## Development accounts

When development seeding is enabled, these accounts share the password supplied through `Seed__Password` or `SEED_PASSWORD`:

| Role | Email |
|---|---|
| Admin | `admin@mypost.local` |
| Courier | `courier@mypost.local` |
| Customer | `customer@mypost.local` |
| Customer | `customer2@mypost.local` |

Use `MP-DEMO-100001` through `MP-DEMO-100006` to explore public tracking and multiple lifecycle states.

## Database migrations

The initial migration is committed under `src/MyPost.Infrastructure/Persistence/Migrations`.

```powershell
dotnet tool restore
dotnet ef database update --project src/MyPost.Infrastructure
```

To create a future migration:

```powershell
dotnet ef migrations add MeaningfulName --project src/MyPost.Infrastructure
```

## Quality checks

Backend:

```powershell
dotnet restore MyPost.sln
dotnet build MyPost.sln --no-restore
dotnet test MyPost.sln --no-build --no-restore
```

Frontend:

```powershell
Set-Location apps/MyPost.Web
npm ci
npm run typecheck
npm run lint
npm test
npm run build
npm run test:e2e
```

Playwright uses an installed Chrome channel and verifies the public experience at the four supported viewport widths plus the shipment-creation flow.

## CLI

The CLI intentionally contains no duplicate domain or persistence rules:

```powershell
dotnet run --project apps/MyPost.Cli -- track MP-DEMO-100001
```

It uses the same database and configuration as the API.

## Security model

- Access tokens are short-lived and kept only in browser memory.
- Refresh tokens are rotated, stored as SHA-256 hashes, and sent in an HttpOnly same-site cookie.
- Role policies protect customer, courier, and administrator routes; application services also enforce resource ownership.
- Public tracking excludes sender identity, phone numbers, street addresses, courier identity, delivery notes, and internal assignment data.
- API errors use Problem Details with a trace ID and do not expose stack traces.
- Local seed credentials and Compose values are development-only; no production secret is stored in the repository.

## Screenshot

![MyPost public tracking experience](docs/assets/mypost-public-home.png)

## Current boundaries

- MyPost is a virtual workflow: it has no carrier, mapping, payment, notification, or real-time vehicle integration.
- Analytics are operational summaries over the application database, not a warehouse pipeline.
- Compose is intended for local evaluation; production deployment still requires managed secrets, TLS, backups, observability, and a deployment-specific migration strategy.
- Browser E2E tests use mocked API responses for deterministic UI coverage; server boundary tests run separately against an in-memory provider.

## Repository history

The original JSON console behavior was mapped to the new domain/application model before its duplicate persistence implementation was removed. See [docs/architecture/legacy-migration.md](docs/architecture/legacy-migration.md). Milestone branches preserve the architecture, domain, persistence, API, web, CLI, testing, and deployment stages for review.
