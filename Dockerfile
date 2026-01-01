# ===========================
# Etapa 1: Build
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas os projetos necessários do monorepo
COPY FCG.Pagamentos/ ./FCG.Pagamentos/

# Restaura e publica a API
RUN dotnet restore "FCG.Pagamentos/FCG.Pagamentos.API/FCG.Pagamentos.API.csproj"
RUN dotnet publish "FCG.Pagamentos/FCG.Pagamentos.API/FCG.Pagamentos.API.csproj" -c Release -o /app/publish

# ===========================
# Etapa 2: Runtime
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Pagamentos.API.dll"]
