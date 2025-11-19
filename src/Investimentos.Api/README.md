# API de Simulação de Investimentos

## 🚀 Como executar

### Opção 1: Docker (Recomendado)

```bash
# Usando Docker Compose
docker-compose up -d --build

# A API estará disponível em: http://localhost:8080
```

Ver [DOCKER.md](../../DOCKER.md) para instruções detalhadas.

### Opção 2: .NET CLI

```bash
cd src/Investimentos.Api
dotnet run
```

A API estará disponível em: `http://localhost:5222`

## 📋 Endpoints Implementados

### Autenticação

#### Login
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
  "expiresAt": "2025-11-17T06:29:54.256349Z"
}
```

**Notas:**
- Token JWT válido por 60 minutos (configurável em `appsettings.json`)
- Verifica se o cliente existe no banco antes de gerar o token
- Token contém claim `clienteId` para identificação

**Development Bypass:**
Em ambiente de desenvolvimento, endpoints protegidos podem ser acessados sem token usando o header:
```
X-Debug-Bypass: 1
```
Isso autentica automaticamente como cliente ID 123 para facilitar testes.

### 1. Health Check
```http
GET /api/health
```
Retorna status 200 OK quando a API está funcionando. **Endpoint público** (não requer autenticação).

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

**⚠️ Requer Autenticação:** Este endpoint requer um token JWT válido no header `Authorization`.

**Validações:**
- `clienteId` > 0
- `valor` > 0
- `prazoMeses` > 0 e ≤ 360
- `tipoProduto` não vazio e ≤ 50 caracteres

**Lógica de Simulação:**
1. Busca produtos elegíveis:
   - `valorMinimoInvestimento` ≤ valor informado
   - `prazoMinimoDias` ≤ prazo convertido para dias
   - `tipo` = tipoProduto (se informado)

2. Seleciona produto com menor risco:
   - Ordem: Baixo → Médio → Alto
   - Em caso de empate, escolhe maior rentabilidade

3. Calcula valor final com juros compostos anuais:
   ```
   valorFinal = valor × (1 + rentabilidade)^(prazoMeses/12)
   ```

4. Salva simulação no banco de dados

5. Retorna resposta:
```json
{
  "produtoValidado": {
    "id": 101,
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
  "dataSimulacao": "2025-11-17T04:00:00Z"
}
```

### 3. Listar Simulações 🔒
```http
GET /api/simulacoes
Authorization: Bearer {token}
```

**⚠️ Requer Autenticação:** Este endpoint requer um token JWT válido no header `Authorization`.

Retorna todas as simulações realizadas, ordenadas da mais recente para a mais antiga:

```json
[
  {
    "id": 1,
    "clienteId": 1,
    "produto": "CDB Caixa 2026",
    "valorInvestido": 10000.00,
    "valorFinal": 11200.00,
    "prazoMeses": 12,
    "dataSimulacao": "2025-11-17T04:00:00Z"
  }
]
```

### 4. Simulações por Produto e Dia
```http
GET /api/simulacoes/por-produto-dia
```

Retorna simulações agrupadas por produto e dia, com estatísticas agregadas:

```json
[
  {
    "produto": "CDB Caixa 2026",
    "data": "2025-11-17",
    "quantidadeSimulacoes": 15,
    "mediaValorFinal": 11050.00
  },
  {
    "produto": "Fundo Multimercado XPTO",
    "data": "2025-11-17",
    "quantidadeSimulacoes": 8,
    "mediaValorFinal": 5700.00
  }
]
```

**Informações retornadas:**
- `produto` - Nome do produto
- `data` - Data das simulações (formato yyyy-MM-dd)
- `quantidadeSimulacoes` - Total de simulações realizadas
- `mediaValorFinal` - Média dos valores finais das simulações

**Ordenação:**
- Primeiro por data (mais recente)
- Depois por nome do produto (alfabética)

### 5. Obter Perfil de Risco
```http
GET /api/perfil-risco/{clienteId}
```

Retorna o perfil de risco calculado dinamicamente para um cliente baseado em seu comportamento:

**Exemplo:**
```bash
GET /api/perfil-risco/1
```

**Resposta:**
```json
{
  "clienteId": 1,
  "nome": "Moderado",
  "pontuacao": 55,
  "descricao": "Perfil equilibrado entre segurança e rentabilidade"
}
```

**Algoritmo de Cálculo (0-100 pontos):**
1. **Volume de investimentos (0-40 pontos)**: Baseado no saldo total (normalizado até R$ 100k)
2. **Frequência de movimentações (0-30 pontos)**: Mais movimentações = mais agressivo
3. **Preferência de liquidez (0-30 pontos)**: Prefere liquidez = conservador (0 pontos), busca rentabilidade = agressivo (+30 pontos)

**Mapeamento de pontuação:**
- 0-40: Conservador
- 41-70: Moderado
- 71-100: Agressivo

### 6. Produtos Recomendados
```http
GET /api/produtos-recomendados/{perfil}
```

Retorna produtos filtrados por compatibilidade de risco, ordenados por rentabilidade (maior primeiro):

**Perfis aceitos:** Conservador, Moderado, Agressivo

**Exemplo para perfil Conservador:**
```bash
GET /api/produtos-recomendados/Conservador
```

**Resposta:**
```json
[
  {
    "id": 101,
    "nome": "CDB Caixa 2026",
    "tipo": "CDB",
    "rentabilidade": 0.12,
    "risco": "Baixo",
    "prazoMinimoDias": 180,
    "valorMinimoInvestimento": 1000.00,
    "liquidezImediata": false
  }
]
```

**Mapeamento de risco por perfil:**
- **Conservador**: Produtos de risco Baixo e Médio
- **Moderado**: Todos os produtos (Baixo, Médio e Alto)
- **Agressivo**: Apenas produtos de risco Alto

### 7. Telemetria

```http
GET /api/telemetria
GET /api/telemetria?dataInicio=2025-11-01&dataFim=2025-11-30
```

Retorna dados de telemetria com volumes e tempos de resposta para cada serviço:

**Resposta:**
```json
{
  "servicos": [
    {
      "nome": "simular-investimento",
      "quantidadeChamadas": 120,
      "mediaTempoRespostaMs": 93.5
    },
    {
      "nome": "perfil-risco",
      "quantidadeChamadas": 80,
      "mediaTempoRespostaMs": 344.0
    },
    {
      "nome": "health",
      "quantidadeChamadas": 50,
      "mediaTempoRespostaMs": 11.5
    }
  ],
  "periodo": {
    "inicio": "2025-11-01",
    "fim": "2025-11-30"
  }
}
```

**Parâmetros opcionais:**
- `dataInicio`: Data inicial do período (formato: yyyy-MM-dd)
- `dataFim`: Data final do período (formato: yyyy-MM-dd)
- Se não informado, retorna dados do último mês

**Como funciona:**
- Middleware `TelemetryMiddleware` captura todas as requisições
- Mede tempo de resposta com `Stopwatch`
- Registra em banco de dados: endpoint, método HTTP, tempo (ms), status code
- Agrupa por serviço e calcula quantidade e média de tempo

## 🧪 Como testar

### Usando curl:

```bash
# Login e obter token
LOGIN_RESPONSE=$(curl -s http://localhost:5222/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 1}')

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token')
echo "Token: $TOKEN"

# Health Check (público)
curl http://localhost:5222/api/health

# Simular investimento (protegido - COM token)
curl -X POST http://localhost:5222/api/simular-investimento \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "clienteId": 1,
    "valor": 10000.00,
    "prazoMeses": 12,
    "tipoProduto": "CDB"
  }'

# Listar simulações (protegido - COM token)
curl http://localhost:5222/api/simulacoes \
  -H "Authorization: Bearer $TOKEN"

# Simular investimento (com bypass de desenvolvimento)
curl -X POST http://localhost:5222/api/simular-investimento \
  -H "Content-Type: application/json" \
  -H "X-Debug-Bypass: 1" \
  -d '{
    "clienteId": 1,
    "valor": 5000.00,
    "prazoMeses": 6,
    "tipoProduto": "Fundo"
  }'

# Obter simulações por produto e dia
curl http://localhost:5222/api/simulacoes/por-produto-dia

# Obter perfil de risco do cliente
curl http://localhost:5222/api/perfil-risco/1

# Obter produtos recomendados para perfil Conservador
curl http://localhost:5222/api/produtos-recomendados/Conservador

# Obter produtos recomendados para perfil Agressivo
curl http://localhost:5222/api/produtos-recomendados/Agressivo

# Obter telemetria
curl http://localhost:5222/api/telemetria

# Obter telemetria de um período específico
curl "http://localhost:5222/api/telemetria?dataInicio=2025-11-01&dataFim=2025-11-30"
```

### Usando os scripts de teste:

```bash
# Testar endpoints principais
./test-api.sh

# Testar perfil de risco e recomendações
./test-perfil-risco.sh

# Testar telemetria (gera dados e consulta)
./test-telemetria.sh

# Testar autenticação JWT
./test-auth.sh
```

### Usando arquivo .http (VS Code REST Client):

Abra o arquivo `test-endpoints.http` no VS Code e clique em "Send Request".

## 📊 Swagger/OpenAPI

Acesse a documentação interativa em:
```
http://localhost:5222/swagger
```

**Autenticação no Swagger:**
1. Clique no botão "Authorize" (cadeado) no topo da página
2. Faça login via `/api/auth/login` para obter um token
3. Insira o token no formato: `Bearer {seu-token-aqui}`
4. Clique em "Authorize"
5. Agora você pode testar os endpoints protegidos diretamente no Swagger

## 💾 Banco de Dados

O projeto usa SQLite com banco de dados local: `investimentos.db`

### Dados iniciais (seed):

**Perfis de Risco:**
- Conservador (0-40 pontos)
- Moderado (41-70 pontos)
- Agressivo (71-100 pontos)

**Produtos:**
1. CDB Caixa 2026
   - Tipo: CDB
   - Rentabilidade: 12% ao ano
   - Risco: Baixo
   - Valor mínimo: R$ 1.000,00
   - Prazo mínimo: 180 dias

2. Fundo Multimercado XPTO
   - Tipo: Fundo
   - Rentabilidade: 18% ao ano
   - Risco: Alto
   - Valor mínimo: R$ 500,00
   - Liquidez imediata

**Cliente exemplo:**
- Nome: João da Silva
- Email: joao.silva@example.com
- Perfil: Moderado
- Saldo: R$ 50.000,00

## 🔧 Tecnologias

- .NET 8
- Entity Framework Core
- SQLite
- AutoMapper
- FluentValidation
- Serilog
- Swagger/OpenAPI
- **JWT Authentication** (Bearer Token)
- System.IdentityModel.Tokens.Jwt

## 🔐 Autenticação e Segurança

### Configuração JWT

O arquivo `appsettings.json` contém as configurações JWT:
```json
{
  "Jwt": {
    "Secret": "sua-chave-secreta-super-segura-com-no-minimo-32-caracteres-para-HS256",
    "Issuer": "InvestimentosApi",
    "Audience": "InvestimentosClient",
    "ExpirationMinutes": 60
  }
}
```

### Endpoints Protegidos

Os seguintes endpoints requerem autenticação JWT (🔒):
- `POST /api/simular-investimento` - Requer token válido
- `GET /api/simulacoes` - Requer token válido

### Endpoints Públicos

Os seguintes endpoints são públicos (não requerem autenticação):
- `POST /api/auth/login` - Gera token JWT
- `GET /api/health` - Health check
- `GET /api/perfil-risco/{clienteId}` - Consulta perfil de risco
- `GET /api/produtos-recomendados/{perfil}` - Lista produtos recomendados
- `GET /api/simulacoes/por-produto-dia` - Estatísticas agregadas
- `GET /api/telemetria` - Dados de telemetria

### Development Bypass (Apenas Desenvolvimento)

Em ambiente de desenvolvimento, use o header `X-Debug-Bypass: 1` para bypassar autenticação:
```bash
curl -X POST http://localhost:5222/api/simular-investimento \
  -H "Content-Type: application/json" \
  -H "X-Debug-Bypass: 1" \
  -d '{"clienteId":1,"valor":10000,"prazoMeses":12,"tipoProduto":"CDB"}'
```

⚠️ **Importante:** O bypass só funciona em ambiente de Development e autentica como cliente ID 123.

