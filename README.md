# DeliveryService - Кур'єрська служба

## Опис
API для відстеження посилок та управління кур'єрами.

## Технології
- .NET 8
- PostgreSQL (або Testcontainers)
- Entity Framework Core
- xUnit, Testcontainers, k6
- GitHub Actions

## Запуск
1. Встановіть PostgreSQL (або використовуйте Docker)
2. Оновіть рядок підключення в `appsettings.json`
3. Застосуйте міграції: `dotnet ef database update`
4. Запустіть API: `dotnet run --project DeliveryService.API`

## Тести
- Модульні: `dotnet test DeliveryService.Tests.Unit`
- Інтеграційні: `dotnet test DeliveryService.Tests.Integration`
- Продуктивність (k6): `k6 run tracking_test.js`
