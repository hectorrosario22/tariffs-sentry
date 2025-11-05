# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and projects
COPY ["HighPerformanceTariffsAPI.sln", "."]
COPY ["src/HighPerformanceTariffsAPI.Api/HighPerformanceTariffsAPI.Api.csproj", "HighPerformanceTariffsAPI.Api/"]
COPY ["src/HighPerformanceTariffsAPI.Application/HighPerformanceTariffsAPI.Application.csproj", "HighPerformanceTariffsAPI.Application/"]
COPY ["src/HighPerformanceTariffsAPI.Domain/HighPerformanceTariffsAPI.Domain.csproj", "HighPerformanceTariffsAPI.Domain/"]
COPY ["src/HighPerformanceTariffsAPI.Infrastructure/HighPerformanceTariffsAPI.Infrastructure.csproj", "HighPerformanceTariffsAPI.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "HighPerformanceTariffsAPI.sln"

# Copy source code
COPY . .

# Build
RUN dotnet build "HighPerformanceTariffsAPI.sln" -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS publish
WORKDIR /src
COPY --from=build /app/build .
RUN dotnet publish "HighPerformanceTariffsAPI.Api/HighPerformanceTariffsAPI.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "HighPerformanceTariffsAPI.Api.dll"]
