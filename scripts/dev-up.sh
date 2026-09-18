#!/bin/bash
# Bring up the full local dev stack: Colima (Docker runtime) -> SQL Server -> backend -> frontend.
# Data persists across restarts (Docker named volume) — this script just restarts the processes.
set -e

echo "==> Starting Colima (Docker runtime)..."
colima status >/dev/null 2>&1 || colima start

echo "==> Starting SQL Server..."
cd "$(dirname "$0")/.."
docker compose up -d sqlserver

echo "==> Waiting for SQL Server to be healthy..."
until [ "$(docker inspect --format='{{.State.Health.Status}}' ecommerce-sqlserver 2>/dev/null)" = "healthy" ]; do
  sleep 2
done
echo "SQL Server is healthy."

echo "==> Starting backend API (https://localhost:7184)..."
(cd backend && dotnet run --project src/ECommerce.Api --launch-profile https &)

echo "==> Starting frontend (http://localhost:4200)..."
(cd frontend && npm start &)

echo ""
echo "All starting up. Give it ~15-20s, then open http://localhost:4200"
