# API de Simulação de Investimentos

API REST desenvolvida em .NET 8 para simulação de investimentos com recomendação de produtos baseada em perfil de risco.

## 🎯 Funcionalidades

- ✅ Simulação de investimentos com cálculo de juros compostos
- ✅ Sistema de perfil de risco (Conservador, Moderado, Agressivo)
- ✅ Recomendação de produtos por perfil
- ✅ Autenticação JWT Bearer Token
- ✅ Telemetria de requisições
- ✅ Health check
- ✅ Documentação Swagger/OpenAPI
- ✅ Testes unitários e de integração (xUnit)
- ✅ Docker e Docker Compose

## 🚀 Quick Start

### Usando Docker (Recomendado)

```bash
# Clone o repositório
git clone <repo-url>
cd desafio-investimentos

# Inicie com Docker Compose
docker-compose up -d --build

# Acesse a API
open http://localhost:8080/swagger
```

### Usando .NET CLI

```bash
# Restaurar dependências
dotnet restore

# Executar a API
cd src/Investimentos.Api
dotnet run

# Acesse a API
open http://localhost:5222/swagger
```

### Executar Testes

```bash
# Todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

## 📦 Estrutura do Projeto

```
desafio-investimentos/
├── src/
│   └── Investimentos.Api/          # Projeto principal da API
│       ├── Controllers/            # Endpoints REST
│       ├── Models/                 # Entidades do domínio
│       ├── DTOs/                   # Data Transfer Objects
│       ├── Services/               # Lógica de negócio
│       ├── Data/                   # Contexto EF Core
│       ├── Middlewares/            # Middlewares customizados
│       ├── Validators/             # FluentValidation
│       └── Mappings/               # AutoMapper profiles
├── tests/
│   └── Investimentos.Tests/        # Testes unitários e integração
│       ├── UnitTests/              # Testes de serviços
│       └── IntegrationTests/       # Testes de endpoints
├── Dockerfile                      # Dockerfile multistage
├── docker-compose.yml              # Orquestração Docker
├── .dockerignore                   # Exclusões do build
└── validate-docker.sh              # Script de validação

```

## 🔐 Autenticação

A API utiliza JWT Bearer Token para proteger endpoints sensíveis.

### Obter Token

```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 1}'
```

### Usar Token

```bash
curl -X POST http://localhost:8080/api/simular-investimento \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": 1,
    "valor": 10000,
    "prazoMeses": 12,
    "tipoProduto": "CDB"
  }'
```

### Development Bypass (Apenas Dev)

Para facilitar testes, use o header `X-Debug-Bypass: 1`:

```bash
curl -X POST http://localhost:8080/api/simular-investimento \
  -H "X-Debug-Bypass: 1" \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 1, "valor": 10000, "prazoMeses": 12}'
```

## 📊 Endpoints Principais

| Método | Endpoint                           | Descrição              | Auth            |
|--------|------------------------------------|------------------------|-----------------|
| `GET`  | `/api/health`                      | Health check           | 🔓 Público      |
| `POST` | `/api/auth/login`                  | Obter token JWT        | 🔓 Público      |
| `POST` | `/api/simular-investimento`        | Simular investimento   | 🔒 Requer token |
| `GET`  | `/api/simulacoes`                  | Listar simulações      | 🔒 Requer token |
| `GET`  | `/api/simulacoes/por-produto-dia`  | Estatísticas agregadas | 🔓 Público      |
| `GET`  | `/api/perfil-risco/{id}`           | Obter perfil de risco  | 🔓 Público      |
| `GET`  | `/api/produtos/recomendacoes/{id}` | Produtos recomendados  | 🔓 Público      |
| `GET`  | `/api/telemetria`                  | Dados de telemetria    | 🔓 Público      |

Ver [src/Investimentos.Api/README.md](src/Investimentos.Api/README.md) para documentação completa dos endpoints.

## 🐳 Docker

### Arquitetura

- **Multistage build** para otimizar tamanho da imagem
- **Imagem base**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Volume**: Persistência do SQLite em `./data`
- **Porta**: 8080:80

### Comandos Úteis

```bash
# Build e iniciar
docker-compose up -d --build

# Ver logs
docker-compose logs -f api

# Parar
docker-compose down

# Reiniciar banco
docker-compose down -v && rm -rf ./data && docker-compose up -d
```

Ver [DOCKER.md](DOCKER.md) para instruções completas.

## 🧪 Testes

O projeto possui **33 testes** cobrindo:

- ✅ Testes unitários de serviços (SimulacaoService, RecomendacaoService)
- ✅ Testes de integração de endpoints
- ✅ Testes de autenticação JWT
- ✅ Testes de validação
- ✅ Testes de persistência no banco

### Executar Testes

```bash
# Todos os testes
dotnet test

# Com verbosidade
dotnet test --verbosity normal

# Apenas unitários
dotnet test --filter "FullyQualifiedName~UnitTests"

# Apenas integração
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### Cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 💾 Banco de Dados

- **SQLite** - Banco leve e portável
- **EF Core 8** - ORM com migrations
- **Seed automático** - Dados iniciais criados na primeira execução

### Entidades

- `Cliente` - Dados do cliente
- `PerfilRisco` - Perfis de risco (Conservador, Moderado, Agressivo)
- `Produto` - Produtos de investimento (CDB, Fundos, etc)
- `Simulacao` - Histórico de simulações
- `TelemetriaRegistro` - Logs de requisições

### Localização do Banco

- **Local**: `src/Investimentos.Api/investimentos.db`
- **Docker**: `/app/data/investimentos.db` (volume em `./data`)

## 🛠️ Tecnologias

| Categoria       | Tecnologia                   |
|-----------------|------------------------------|
| Framework       | .NET 8, ASP.NET Core         |
| ORM             | Entity Framework Core 8      |
| Banco           | SQLite                       |
| Validação       | FluentValidation             |
| Mapeamento      | AutoMapper                   |
| Autenticação    | JWT Bearer Token             |
| Logging         | Serilog                      |
| Testes          | xUnit, FluentAssertions, Moq |
| Documentação    | Swagger/OpenAPI              |
| Containerização | Docker, Docker Compose       |

## 📝 Scripts de Teste

O projeto inclui scripts bash para testar os endpoints:

```bash
# Testar API principal
./test-api.sh

# Testar perfil de risco
./test-perfil-risco.sh

# Testar telemetria
./test-telemetria.sh

# Testar autenticação
./test-auth.sh

# Validar Docker
./validate-docker.sh
```

## 🔧 Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=investimentos.db"
  },
  "Jwt": {
    "Secret": "sua-chave-secreta-super-segura",
    "ExpirationMinutes": 60
  }
}
```

### Variáveis de Ambiente (Docker)

| Variável                               | Descrição      | Padrão                                   |
|----------------------------------------|----------------|------------------------------------------|
| `ASPNETCORE_ENVIRONMENT`               | Ambiente       | `Production`                             |
| `ASPNETCORE_URLS`                      | URL de escuta  | `http://+:80`                            |
| `ConnectionStrings__DefaultConnection` | String conexão | `Data Source=/app/data/investimentos.db` |

## 📖 Documentação

- **API**: [src/Investimentos.Api/README.md](src/Investimentos.Api/README.md)
- **Docker**: [DOCKER.md](DOCKER.md)
- **Swagger**: http://localhost:8080/swagger (quando em execução)

## 🎯 Algoritmos

### Cálculo de Juros Compostos

```
VF = VP × (1 + i)^(n/12)

Onde:
- VF = Valor Final
- VP = Valor Presente (investimento inicial)
- i = Taxa de rentabilidade anual
- n = Prazo em meses
```

### Perfil de Risco

Pontuação de 0-100 baseada em:
- **Volume** (0-40 pts): Saldo total normalizado até R$ 100k
- **Frequência** (0-30 pts): Número de movimentações × 3
- **Liquidez** (0-30 pts): Prefere liquidez = 0, busca rentabilidade = 30

**Mapeamento:**
- 0-40: Conservador
- 41-70: Moderado
- 71-100: Agressivo

## 📄 Licença

Este projeto é um desafio técnico para fins de avaliação.