# E-Commerce Platform

Full-stack e-commerce platform: ASP.NET Core Web API (.NET 10) + Angular (standalone, signals) + SQL Server.

See [AGENTS.md](./AGENTS.md) for setup commands, architecture, and conventions (this file is the single source of truth for both humans and AI coding agents working in this repo).

## Status

🚧 Under active development. See open issues/PRs for current progress against the MVP roadmap.

## Quick start

```bash
# 1. Start local SQL Server
docker compose up -d sqlserver

# 2. Backend
cd backend
dotnet restore
dotnet ef database update --project src/ECommerce.Infrastructure --startup-project src/ECommerce.Api
dotnet run --project src/ECommerce.Api

# 3. Frontend (new terminal)
cd frontend
npm install
npm start
```

## License

Private / proprietary — all rights reserved.
