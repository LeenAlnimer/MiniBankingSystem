# =========================
# Build Stage
# =========================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["MiniBankingSystem.Api/MiniBankingSystem.Api.csproj", "MiniBankingSystem.Api/"]
COPY ["MiniBankingSystem.Application/MiniBankingSystem.Application.csproj", "MiniBankingSystem.Application/"]
COPY ["MiniBankingSystem.Domain/MiniBankingSystem.Domain.csproj", "MiniBankingSystem.Domain/"]
COPY ["MiniBankingSystem.Infrastructure/MiniBankingSystem.Infrastructure.csproj", "MiniBankingSystem.Infrastructure/"]

RUN dotnet restore "MiniBankingSystem.Api/MiniBankingSystem.Api.csproj"

COPY . .

RUN dotnet build "MiniBankingSystem.Api/MiniBankingSystem.Api.csproj" -c Release -o /app/build

RUN dotnet publish "MiniBankingSystem.Api/MiniBankingSystem.Api.csproj" -c Release -o /app/publish


# =========================
# Runtime Stage
# =========================

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MiniBankingSystem.Api.dll"]