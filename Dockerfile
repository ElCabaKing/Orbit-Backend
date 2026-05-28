FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Orbit.slnx .
COPY src/Orbit.ApiWeb/Orbit.ApiWeb.csproj src/Orbit.ApiWeb/
COPY src/Orbit.Application/Orbit.Application.csproj src/Orbit.Application/
COPY src/Orbit.Domain/Orbit.Domain.csproj src/Orbit.Domain/
COPY src/Orbit.Infrastructure/Orbit.Infrastructure.csproj src/Orbit.Infrastructure/
COPY src/Orbit.Shared/Orbit.Shared.csproj src/Orbit.Shared/

RUN dotnet restore src/Orbit.ApiWeb/Orbit.ApiWeb.csproj

COPY src/ src/

RUN dotnet publish src/Orbit.ApiWeb/Orbit.ApiWeb.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["/bin/bash", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet Orbit.ApiWeb.dll"]