# API de Simulação de Investimentos

API REST desenvolvida em .NET 8 para simulação de investimentos com recomendação de produtos baseada em perfil de risco comportamental.

## 🎯 Funcionalidades

- ✅ **Simulação de investimentos** com cálculo de juros compostos
- ✅ **Sistema de perfil de risco** (Conservador, Moderado, Agressivo) baseado em comportamento
- ✅ **Recomendação de produtos** por perfil de risco
- ✅ **Autenticação JWT** Bearer Token com bypass de desenvolvimento
- ✅ **Telemetria** de requisições com métricas de performance
- ✅ **Health check** e monitoramento
- ✅ **Documentação Swagger/OpenAPI** interativa
- ✅ **37 testes** unitários e de integração (xUnit + FluentAssertions + Moq)
- ✅ **Docker e Docker Compose** para deploy containerizado
- ✅ **Scripts automatizados** de teste e validação

## 🚀 Quick Start

### Opção 1: Docker (Recomendado)

```bash
# Clone o repositório
git clone <repo-url>
cd desafio-investimentos

# Inicie com Docker Compose
docker-compose up -d --build

# Acesse a API
open http://localhost:8080/swagger
```

A API estará disponível em:
- **Base URL**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/api/health

### Opção 2: .NET CLI

```bash
# Restaurar dependências
dotnet restore

# Executar a API
cd src/Investimentos.Api
dotnet run

# Acesse a API
open http://localhost:5222/swagger
```

A API estará disponível em:
- **Base URL**: http://localhost:5222
- **Swagger UI**: http://localhost:5222/swagger

### Executar Testes

```bash
# Todos os testes (37 testes)
dotnet test

# Com verbosidade
dotnet test --verbosity normal

# Com cobertura
dotnet test /p:CollectCoverage=true

# Apenas testes unitários
dotnet test --filter "FullyQualifiedName~UnitTests"

# Apenas testes de integração
dotnet test --filter "FullyQualifiedName~IntegrationTests"
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
│   └── Investimentos.Tests/        # 37 testes
│       ├── UnitTests/              # Testes de serviços
│       └── IntegrationTests/       # Testes de endpoints
└── README.md                       # Documentação principal
```

## 📊 Endpoints da API

### 🔑 Autenticação JWT

```http
POST /api/auth/login
Content-Type: application/json

{
  "clienteId": 1
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "clienteId": 1,
  "expiresAt": "2025-11-21T06:29:54Z"
}
```

**Usando o token:**
```bash
curl -X POST http://localhost:8080/api/simular-investimento \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 1, "valor": 10000, "prazoMeses": 12}'
```

**Development Bypass:** Use `X-Debug-Bypass: 1` para acessar endpoints protegidos sem token em dev.

### Resumo dos Endpoints

| Método | Endpoint                              | Descrição                       | Auth            |
|--------|---------------------------------------|---------------------------------|-----------------|
| `GET`  | `/api/health`                         | Health check                    | 🔓 Público      |
| `POST` | `/api/auth/login`                     | Obter token JWT                 | 🔓 Público      |
| `POST` | `/api/simular-investimento`           | Simular investimento            | 🔒 Requer token |
| `GET`  | `/api/simulacoes`                     | Listar simulações               | 🔒 Requer token |
| `GET`  | `/api/simulacoes/por-produto-dia`     | Estatísticas agregadas          | 🔓 Público      |
| `GET`  | `/api/investimentos/{clienteId}`      | Histórico de investimentos      | 🔓 Público      |
| `GET`  | `/api/perfil-risco/{clienteId}`       | Obter perfil de risco           | 🔓 Público      |
| `GET`  | `/api/produtos-recomendados/{perfil}` | Produtos recomendados           | 🔓 Público      |
| `GET`  | `/api/telemetria`                     | Dados de telemetria             | 🔓 Público      |

### 1. Health Check
```http
GET /api/health
```
Retorna status 200 OK quando a API está funcionando. Endpoint público.

### 2. Simular Investimento 🔒
```http
POST /api/simular-investimento
Content-Type: application/json
Authorization: Bearer {token}

{
  "clienteId": 1,
  "valor": 10000.00,
  "prazoMeses": 12,
  "tipoProduto": "CDB"
}
```

**Validações:**
- `clienteId` > 0
- `valor` > 0
- `prazoMeses` > 0 e ≤ 360
- `tipoProduto` não vazio e ≤ 50 caracteres

**Lógica de Simulação:**
1. Busca produtos elegíveis (valor mínimo, prazo mínimo, tipo)
2. Seleciona produto com menor risco (Baixo → Médio → Alto)
3. Calcula valor final: `VF = VP × (1 + i)^(n/12)`
4. Salva simulação no banco
5. Retorna resultado

**Resposta:**
```json
{
  "produtoValidado": {
    "id": 1,
    "nome": "CDB Caixa 2026",
    "tipo": "CDB",
    "rentabilidade": 0.12,
    "risco": "Baixo"
  },
  "resultadoSimulacao": {
    "valorFinal": 11200.00,
    "rentabilidadeEfetiva": 0.12,
    "prazoMeses": 12
  },
  "dataSimulacao": "2025-11-21T04:00:00Z"
}
```

### 3. Listar Simulações 🔒
```http
GET /api/simulacoes
Authorization: Bearer {token}
```

Retorna todas as simulações ordenadas da mais recente para mais antiga.

**Resposta:**
```json
[
  {
    "id": 1,
    "clienteId": 1,
    "produto": "CDB Caixa 2026",
    "valorInvestido": 10000.00,
    "valorFinal": 11200.00,
    "prazoMeses": 12,
    "dataSimulacao": "2025-11-21T04:00:00Z"
  }
]
```

### 4. Simulações por Produto e Dia
```http
GET /api/simulacoes/por-produto-dia
```

Retorna estatísticas agregadas por produto e dia.

**Resposta:**
```json
[
  {
    "produto": "CDB Caixa 2026",
    "data": "2025-11-21",
    "quantidadeSimulacoes": 15,
    "mediaValorFinal": 11050.00
  }
]
```

### 5. Histórico de Investimentos por Cliente
```http
GET /api/investimentos/{clienteId}
```

Retorna histórico de investimentos (simulações realizadas) de um cliente específico.

**Exemplo:**
```bash
GET /api/investimentos/1
```

**Resposta:**
```json
[
  {
    "id": 1,
    "tipo": "CDB",
    "valor": 10000.00,
    "rentabilidade": 0.12,
    "data": "2025-11-21"
  }
]
```

### 6. Obter Perfil de Risco
```http
GET /api/perfil-risco/{clienteId}
```

Calcula perfil de risco baseado em comportamento do cliente.

**Resposta:**
```json
{
  "clienteId": 1,
  "nome": "Moderado",
  "pontuacao": 65,
  "descricao": "Perfil equilibrado entre segurança e rentabilidade."
}
```

**Algoritmo de Cálculo (0-100 pontos):**
- **Volume** (0-40 pts): Saldo total normalizado até R$ 100.000,00
- **Frequência** (0-30 pts): Número de movimentações × 3
- **Liquidez** (0-30 pts): Preferência por liquidez vs rentabilidade

**Mapeamento:**
- 0-40: Conservador
- 41-70: Moderado  
- 71-100: Agressivo

### 7. Produtos Recomendados
```http
GET /api/produtos-recomendados/{perfil}
```

Retorna produtos filtrados por risco, ordenados por rentabilidade (maior primeiro).

**Perfis:** `Conservador`, `Moderado`, `Agressivo`

**Resposta:**
```json
[
  {
    "id": 1,
    "nome": "CDB Caixa 2026",
    "tipo": "CDB",
    "rentabilidade": 0.12,
    "risco": "Baixo"
  }
]
```

**Mapeamento de risco:**
- **Conservador**: Risco Baixo apenas
- **Moderado**: Risco Baixo + Médio
- **Agressivo**: Todos os riscos (Baixo + Médio + Alto)

### 8. Telemetria
```http
GET /api/telemetria
GET /api/telemetria?dataInicio=2025-11-01&dataFim=2025-11-30
```

Retorna métricas de volume e performance por serviço.

**Resposta:**
```json
{
  "servicos": [
    {
      "nome": "simular-investimento",
      "quantidadeChamadas": 120,
      "mediaTempoRespostaMs": 93.5
    }
  ],
  "periodo": {
    "inicio": "2025-11-01",
    "fim": "2025-11-30"
  }
}
```

## 🐳 Docker

```bash
# Build e iniciar
docker-compose up -d --build

# Ver logs
docker-compose logs -f api

# Parar
docker-compose down

# Reiniciar (limpa banco)
docker-compose down -v && rm -rf ./data && docker-compose up -d
```

A API estará em http://localhost:8080. O banco SQLite é persistido em `./data/investimentos.db`.

## 🧪 Testes

**37 testes** - unitários, integração, autenticação e validação.

```bash
# Executar todos os testes
dotnet test

# Testes unitários apenas
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Scripts de Teste

Scripts bash para validação completa da API (requer `curl` e opcionalmente `jq`):

```bash
# Teste completo
./scripts/test-all-endpoints.sh

# Testes específicos
./scripts/test-api.sh              # Endpoints principais
./scripts/test-auth.sh             # Autenticação JWT
./scripts/test-perfil-risco.sh     # Perfil de risco
./scripts/test-telemetria.sh       # Telemetria
./scripts/validate-docker.sh       # Validação Docker
./scripts/clean-project.sh         # Limpeza para export
```

## 💾 Banco de Dados

- **SQLite** - Banco leve e portável para desenvolvimento
- **EF Core 8** - ORM com migrations automáticas
- **Seed automático** - Dados iniciais criados na primeira execução

### Entidades

- **Cliente** - Dados do cliente (nome, email, saldo, perfil)
- **PerfilRisco** - Perfis de risco (Conservador, Moderado, Agressivo)
- **Produto** - Produtos de investimento (CDB, Fundos, LCI, etc)
- **Simulacao** - Histórico de simulações realizadas
- **TelemetriaRegistro** - Logs de requisições com métricas

### Dados de Seed (Iniciais)

**Perfis de Risco:**
- Conservador (0-40 pontos) - "Prioriza segurança e baixo risco"
- Moderado (41-70 pontos) - "Perfil equilibrado entre segurança e rentabilidade"
- Agressivo (71-100 pontos) - "Busca alta rentabilidade, aceita maior risco"

**Produtos:**
1. **CDB CAIXA 2026**
   - Tipo: CDB
   - Rentabilidade: 12% ao ano
   - Risco: Baixo
   - Valor mínimo: R$ 1.000,00
   - Prazo mínimo: 180 dias
   - Liquidez: Não imediata
   - Perfil recomendado: Conservador

2. **LCI CAIXA**
   - Tipo: LCI
   - Rentabilidade: 15% ao ano
   - Risco: Médio
   - Valor mínimo: R$ 2.000,00
   - Prazo mínimo: 90 dias
   - Liquidez: Não imediata
   - Perfil recomendado: Moderado

3. **Fundo Multimercado XPTO**
   - Tipo: Fundo
   - Rentabilidade: 18% ao ano
   - Risco: Alto
   - Valor mínimo: R$ 500,00
   - Prazo mínimo: Sem prazo
   - Liquidez: Imediata
   - Perfil recomendado: Agressivo

**Cliente de Exemplo:**
- ID: 1
- Nome: João da Silva
- Email: joao.silva@example.com
- Perfil: Moderado
- Saldo: R$ 50.000,00
- Movimentações: 10
- Preferência: Busca rentabilidade

### Localização do Banco

- **Local (.NET CLI)**: `src/Investimentos.Api/investimentos.db`
- **Docker**: `/app/data/investimentos.db` (volume montado em `./data`)