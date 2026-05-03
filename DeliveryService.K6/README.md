# DeliveryService k6 Tests

Standalone k6 performance test suite for DeliveryService API.

## Prerequisites
- API must be running.
- k6 must be installed.
- Use real package data from the current database.

Set the API base URL:
```powershell
$env:BASE_URL = "http://localhost:5166"
```

## Smoke Test
Quick check that the tracking endpoint and updates endpoint are reachable for one package.

```powershell
$env:TRACKING_CODE = "<existing-tracking-code>"
k6 run .\DeliveryService.K6\smoke_test.js
```

## Load Test
Sustained read load for `GET /api/packages/{trackingCode}`.

```powershell
$env:TRACKING_CODE = "<existing-tracking-code>"
$env:TRACKING_VUS = "50"
$env:RAMP_UP = "30s"
$env:HOLD_FOR = "1m"
$env:RAMP_DOWN = "30s"
k6 run .\DeliveryService.K6\load_test.js
```

## Stress Test
Concurrent status updates through `POST /api/packages/{id}/update`.

Use package ids that are in the correct source status for `TARGET_STATUS`.
For default `PickedUp`, use packages currently in `Created` status.

```powershell
$env:PACKAGE_IDS = "<package-id-1>,<package-id-2>,<package-id-3>"
$env:TARGET_STATUS = "PickedUp"
$env:UPDATE_VUS = "25"
$env:ITERATIONS = "25"
k6 run .\DeliveryService.K6\stress_test.js
```

By default the stress test runs one update per provided package id. Increase `ITERATIONS` only when the target packages and target status transitions make repeated requests valid.
