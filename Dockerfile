# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["src/Investimentos.Api/Investimentos.Api.csproj", "src/Investimentos.Api/"]
RUN dotnet restore "src/Investimentos.Api/Investimentos.Api.csproj"

# Copiar todo o código fonte e compilar
COPY . .
WORKDIR "/src/src/Investimentos.Api"
RUN dotnet build "Investimentos.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Investimentos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Criar diretório para o banco de dados
RUN mkdir -p /app/data

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Expor porta 80
EXPOSE 80

# Definir variável de ambiente para SQLite
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/investimentos.db"

ENTRYPOINT ["dotnet", "Investimentos.Api.dll"]
