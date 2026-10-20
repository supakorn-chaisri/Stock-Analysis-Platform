FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["src/Api/StockAnalysis.Api.csproj", "src/Api/"]
COPY ["src/Domain/StockAnalysis.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/StockAnalysis.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Api/StockAnalysis.Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "StockAnalysis.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StockAnalysis.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockAnalysis.Api.dll"]