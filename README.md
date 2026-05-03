# DeliveryService - Courier Delivery API

## Overview
API for creating packages, tracking delivery status, and storing delivery update history.

## Technologies
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- xUnit
- Testcontainers.PostgreSql
- k6
- GitHub Actions

## Run
1. Start PostgreSQL or use Docker.
2. Configure `DefaultConnection` in `DeliveryService.API/appsettings.json`.
3. Apply migrations:
   ```powershell
   dotnet ef database update --project DeliveryService.Infrastructure --startup-project DeliveryService.API
   ```
4. Run the API:
   ```powershell
   dotnet run --project DeliveryService.API
   ```

## Tests
Run unit tests:
```powershell
dotnet test DeliveryService.Tests.Unit
```

Run integration tests. Docker must be running because these tests use Testcontainers:
```powershell
dotnet test DeliveryService.Tests.Integration
```

## k6 Performance Tests
k6 tests are stored as a standalone suite in `DeliveryService.K6`.

- Smoke: `k6 run .\DeliveryService.K6\smoke_test.js`
- Load: `k6 run .\DeliveryService.K6\load_test.js`
- Stress: `k6 run .\DeliveryService.K6\stress_test.js`

See [DeliveryService.K6/README.md](DeliveryService.K6/README.md) for required environment variables and examples.
