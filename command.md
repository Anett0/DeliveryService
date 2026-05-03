# Команди для проекту DeliveryService

## Запуск юніт тестів
```
dotnet test DeliveryService.Tests.Unit
```

## Запуск інтеграційних тестів
```
dotnet test DeliveryService.Tests.Integration
```

## Збірка проекту
```
dotnet build DeliveryService.sln
```

## Запуск API додатку
```
cd DeliveryService.API
dotnet run
```
Після запуску Swagger буде доступний за адресою: http://localhost:5166/swagger

## Запуск Docker
```
docker-compose up --build
```

## Запуск k6 тестів
### Smoke тест
```
$env:BASE_URL = "http://localhost:5166"
$env:TRACKING_CODE = "TEST123"
k6 run .\DeliveryService.K6\smoke_test.js
```

### Load тест
```
$env:BASE_URL = "http://localhost:5166"
$env:TRACKING_CODE = "TEST123"
$env:TRACKING_VUS = "50"
$env:RAMP_UP = "30s"
$env:HOLD_FOR = "1m"
$env:RAMP_DOWN = "30s"
k6 run .\DeliveryService.K6\load_test.js
```

### Stress тест
```
$env:BASE_URL = "http://localhost:5166"
$env:PACKAGE_IDS = "<package-id-1>,<package-id-2>"
$env:TARGET_STATUS = "PickedUp"
$env:UPDATE_VUS = "25"
$env:ITERATIONS = "25"
k6 run .\DeliveryService.K6\stress_test.js
```

## Міграції бази даних
```
cd DeliveryService.API
dotnet ef database update
```

## Створення міграції (якщо потрібно)
```
cd DeliveryService.API
dotnet ef migrations add <MigrationName>
```