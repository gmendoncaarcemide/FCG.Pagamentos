# ===========================
# Etapa 1: Build
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os csproj primeiro para aproveitar cache do Docker
COPY ["FCG.Pagamentos.API/FCG.Pagamentos.API.csproj", "FCG.Pagamentos.API/"]
COPY ["FCG.Pagamentos.Functions/FCG.Pagamentos.Functions.csproj", "FCG.Pagamentos.Functions/"]
COPY ["FCG.Pagamentos.Application/FCG.Pagamentos.Application.csproj", "FCG.Pagamentos.Application/"]
COPY ["FCG.Pagamentos.Domain/FCG.Pagamentos.Domain.csproj", "FCG.Pagamentos.Domain/"]
COPY ["FCG.Pagamentos.Infrastructure/FCG.Pagamentos.Infrastructure.csproj", "FCG.Pagamentos.Infrastructure/"]

# Restaura dependências da API
RUN dotnet restore "FCG.Pagamentos.API/FCG.Pagamentos.API.csproj"

# Copia todo o código
COPY . .

# Faz o build da API
WORKDIR "/src/FCG.Pagamentos.API"
RUN dotnet publish -c Release -o /app/publish

# ===========================
# Etapa 2: Runtime
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Pagamentos.API.dll"]
