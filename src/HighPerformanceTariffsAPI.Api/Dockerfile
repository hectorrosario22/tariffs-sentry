# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["HighPerformanceTariffsAPI.sln", "."]
COPY ["src/HighPerformanceTariffsAPI.Api/HighPerformanceTariffsAPI.Api.csproj", "src/HighPerformanceTariffsAPI.Api/"]
COPY ["src/HighPerformanceTariffsAPI.Application/HighPerformanceTariffsAPI.Application.csproj", "src/HighPerformanceTariffsAPI.Application/"]
COPY ["src/HighPerformanceTariffsAPI.Domain/HighPerformanceTariffsAPI.Domain.csproj", "src/HighPerformanceTariffsAPI.Domain/"]
COPY ["src/HighPerformanceTariffsAPI.Infrastructure/HighPerformanceTariffsAPI.Infrastructure.csproj", "src/HighPerformanceTariffsAPI.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "HighPerformanceTariffsAPI.sln"

# Copy entire source code
COPY . .

# Publish the API project
RUN dotnet publish "src/HighPerformanceTariffsAPI.Api/HighPerformanceTariffsAPI.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "HighPerformanceTariffsAPI.Api.dll"]
