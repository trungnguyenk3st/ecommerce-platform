# AGENTS.md

Single source of truth for humans and AI coding agents working in this repo. Read this before making changes.

## What this is

A full-stack e-commerce platform: customer storefront + admin back office.

- **Backend**: ASP.NET Core Web API (.NET 10), C#, EF Core, SQL Server, JWT auth, Clean Architecture.
- **Frontend**: Angular 22 (standalone components, signals, zoneless), TypeScript, Bootstrap 5.
- **Payments**: VNPay (sandbox) integration for online checkout, plus cash-on-delivery.

## Repository layout

```
backend/
  ECommerce.sln
  src/
    ECommerce.Domain/          # Entities, enums, domain exceptions — no external dependencies
    ECommerce.Application/     # DTOs, service interfaces + implementations, FluentValidation validators
    ECommerce.Infrastructure/  # EF Core DbContext, Identity, JWT, VNPay — implements Application interfaces
    ECommerce.Api/             # Controllers, Program.cs, middleware, DB seeding
frontend/
  src/app/
    core/                      # Services (HTTP), interceptors, guards, models — singletons, no UI
    shared/                    # Reusable standalone components (header, footer, product-card, ...)
    features/                  # One folder per route/page area (catalog, cart, checkout, admin, ...)
docker-compose.yml             # Local SQL Server only
```

### Backend dependency direction

`Api → Application + Infrastructure → Domain`. `Application` never references `Infrastructure` or
ASP.NET Core packages directly — it depends only on interfaces (`IApplicationDbContext`,
`IIdentityService`, `IJwtTokenService`, `IVnPayService`, `ICurrentUserService`), all defined in
`Application/Common/Interfaces`. `Infrastructure` implements those interfaces. This is what lets
`Application`'s business logic be tested and reasoned about without EF Core or ASP.NET Identity in
the picture. If you add a service that needs "the database" or "the current user," inject the
interface, not the concrete Infrastructure type.

Each feature under `Application/<Feature>/` follows the same shape: `<Feature>Dtos.cs`,
`<Feature>Validators.cs`, `I<Feature>Service.cs`, `<Feature>Service.cs`. Follow this when adding a
new feature area instead of introducing a different pattern.

## Setup & run

### Prerequisites

- .NET 10 SDK
- Node.js 20+ and npm (Angular 22 requires a current LTS; do not use Node 18 or earlier)
- Docker (for local SQL Server) — or point `ConnectionStrings:DefaultConnection` at any reachable
  SQL Server instance

### First-time setup

```bash
# 1. SQL Server
docker compose up -d sqlserver

# 2. Backend secrets (gitignored — this file is not committed)
cp backend/src/ECommerce.Api/appsettings.Development.json.example backend/src/ECommerce.Api/appsettings.Development.json
# edit it: set a random Jwt:Secret (32+ chars), and VNPay sandbox TmnCode/HashSecret if testing payments

# 3. Backend
cd backend
dotnet restore
dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.Api
dotnet run --project src/ECommerce.Api

# 4. Frontend (new terminal)
cd frontend
npm install
npm start
```

- API: `https://localhost:7184` (Swagger UI at `/swagger` in Development)
- Frontend: `http://localhost:4200`

The API seeds roles, a demo admin (`admin@ecommerce.local` / `Admin@12345`), a small catalog, and
a `WELCOME10` coupon on first run — see `ECommerce.Api/Persistence/DbSeeder.cs`. Seeding runs as a
background task after Kestrel starts listening, so the API stays reachable (Swagger, health) even
if the database is briefly unavailable — it retries what it can and logs the rest; it does not
crash the process.

### Running tests

```bash
cd backend && dotnet test
cd frontend && npm test          # vitest, watch=false in CI
```

## Conventions

- **Backend**: nullable reference types on, records for DTOs, FluentValidation for input validation
  (invoked explicitly inside services — `ValidateAndThrowAsync`, not a MediatR pipeline). Domain
  entities enforce their own invariants (e.g. `Product.DecreaseStock` throws `DomainException` on
  insufficient stock) rather than leaving that to callers.
- **Errors**: `ECommerce.Api.Middleware.ExceptionHandlingMiddleware` maps exceptions to a consistent
  JSON shape (`{ status, title, errors, traceId }`). Throw `NotFoundException`, `DomainException`,
  `UnauthorizedException`, `ForbiddenAccessException`, or `AppValidationException` (or let
  FluentValidation throw) from services — don't `try/catch` and return raw error strings from
  controllers.
- **Frontend**: standalone components only, no NgModules. State lives in services as signals
  (`CartService`, `WishlistService`, `AuthService`) — not a global store library. Use `inject()` for
  DI in components with field initializers that reference other injected services (constructor
  parameter properties are not yet assigned when class field initializers run — see git history for
  a class of bugs this caused early on). Reactive Forms (`FormBuilder.nonNullable`) for anything
  more than a single input.
- **Money**: stored as `decimal` end-to-end, formatted client-side with `Intl.NumberFormat('vi-VN',
  { style: 'currency', currency: 'VND' })`. VND has no minor unit — don't multiply/divide by 100
  except inside `VnPayService`, where VNPay's API specifically requires amounts ×100.

## Payments (VNPay)

`ECommerce.Infrastructure/Services/VnPayService.cs` builds the signed checkout redirect URL and
validates the signed callback (HMAC-SHA512 over sorted query params, per VNPay's sandbox docs).
Register a free sandbox merchant at https://sandbox.vnpayment.vn to get a `TmnCode`/`HashSecret` for
local testing. The browser return URL (`Jwt:ReturnUrl` / `VnPay:ReturnUrl` in config) points at the
Angular route `/checkout/payment-result`, which forwards the `vnp_*` query params back to
`GET /api/payments/vnpay/return` for signature validation — VNPay's server-to-server IPN
(`/api/payments/vnpay/ipn`) is the authoritative confirmation and hits the backend directly.

## Known gaps / next steps

This is a solid, working MVP end-to-end (auth, catalog, cart, checkout, orders, wishlist, reviews,
coupons, admin dashboard/CRUD) but is not a finished, audited production system. Before treating it
as one:

- No automated test suite yet beyond the default scaffolded specs — business logic in
  `Application/*/**Service.cs` is the highest-value place to add unit tests first.
- `System.Security.Cryptography.Xml` (a transitive dependency pulled in via ASP.NET Core Identity)
  is flagged by `dotnet list package --vulnerable` even at its latest version — this is an upstream
  advisory-metadata issue, not something fixable by upgrading further; re-check periodically.
- No rate limiting on `/api/auth/*`.
- No image upload — product/category images are URLs only (point them at any public image host for
  a demo).
- No CI pipeline configured yet.
