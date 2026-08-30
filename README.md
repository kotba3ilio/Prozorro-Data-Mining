# Prozorro Data Mining

Application for importing, storing, and analyzing Prozorro tender data.

## Structure

- `src/ProzorroDataMining.Api` - ASP.NET Core Web API entry point.
- `src/ProzorroDataMining.Application` - application layer for use cases and contracts.
- `src/ProzorroDataMining.Domain` - domain layer.
- `src/ProzorroDataMining.Infrastructure` - infrastructure integrations.
- `src/ProzorroDataMining.Web` - React dashboard.

## Run With Docker Compose

```powershell
docker compose up --build
```

Services:

- Frontend: `http://localhost:8080`
- API: `http://localhost:5095`
- PostgreSQL: `localhost:5433`

The compose file uses local development credentials for PostgreSQL only. Override them through environment variables or a compose override file for other environments.

## Run API

```powershell
dotnet run --project src\ProzorroDataMining.Api\ProzorroDataMining.Api.csproj
```

Swagger opens at `http://localhost:5095/swagger`.

## Run Frontend

```powershell
cd src\ProzorroDataMining.Web
npm install
npm run dev
```

The frontend expects the API at `http://localhost:5095`.
