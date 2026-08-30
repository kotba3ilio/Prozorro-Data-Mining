FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ProzorroDataMining.slnx ./
COPY dotnet-tools.json ./
COPY src/ProzorroDataMining.Api/ProzorroDataMining.Api.csproj src/ProzorroDataMining.Api/
COPY src/ProzorroDataMining.Application/ProzorroDataMining.Application.csproj src/ProzorroDataMining.Application/
COPY src/ProzorroDataMining.Domain/ProzorroDataMining.Domain.csproj src/ProzorroDataMining.Domain/
COPY src/ProzorroDataMining.Infrastructure/ProzorroDataMining.Infrastructure.csproj src/ProzorroDataMining.Infrastructure/

RUN dotnet restore src/ProzorroDataMining.Api/ProzorroDataMining.Api.csproj

COPY src ./src
RUN dotnet publish src/ProzorroDataMining.Api/ProzorroDataMining.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProzorroDataMining.Api.dll"]