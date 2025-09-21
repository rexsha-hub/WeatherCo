
# WeatherCo – 3‑tier sample (UI + Azure Functions Middle Tier + Producer API)

This repo contains three .NET 8 projects that satisfy the assignment:
- **WeatherCo.Producer** – ASP.NET Core Web API that *produces* a 5‑day forecast.
- **WeatherCo.MiddleTier** – Azure Functions (Isolated) that accepts the filter and calls the Producer.
- **WeatherCo.UI** – Lightweight ASP.NET Core web app that serves a tiny UI and proxies to the Middle Tier.

The UI lets you filter by date and displays the forecast list.

## Quick start (local)

Requirements: .NET 8 SDK and Azure Functions Core Tools v4.

Terminal 1 – Producer API (port 5191):
```bash
cd WeatherCo.Producer
dotnet restore
dotnet run
```

Terminal 2 – Middle Tier (port 7071):
```bash
cd WeatherCo.MiddleTier
# Point the middle tier at the Producer URL
set PRODUCER_BASEURL=http://localhost:5191
# macOS/Linux: export PRODUCER_BASEURL=http://localhost:5191
dotnet restore
func start
```

Terminal 3 – UI (port 5080):
```bash
cd WeatherCo.UI
# Point the UI at the middle tier URL
set MIDDLE_TIER_BASEURL=http://localhost:7071/api
# macOS/Linux: export MIDDLE_TIER_BASEURL=http://localhost:7071/api
dotnet restore
dotnet run --urls http://localhost:5080
```

Then open http://localhost:5080 in your browser.

## Endpoints

- Producer:
  - `GET /forecast` -> 5‑day list
  - `GET /forecast/{yyyy-MM-dd}` -> single day (404 if not in next 5 days)

- MiddleTier (Azure Functions):
  - `GET /api/forecast` (optional `?date=yyyy-MM-dd`)

- UI app:
  - Serves static page at `/`
  - Proxy endpoint: `GET /api/forecast` (optional `?date=yyyy-MM-dd`)

## Tests
Minimal unit tests for the Producer service:
```bash
cd WeatherCo.Tests
dotnet test
```

## Azure deployment hint (Middle Tier)
1) Create a Function App (Isolated, .NET 8).  
2) Configure app setting `PRODUCER_BASEURL` to your Producer API URL (public).  
3) `func azure functionapp publish <your-func-app-name>` or use GitHub Actions.

## Notes
- Deterministic pseudo‑random generation seeded by the date guarantees stable results per date.
- Summary is intentionally decoupled from temperature, as required.
- CORS is wide open for simplicity. Lock it down for prod.
