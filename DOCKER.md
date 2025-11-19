# Docker - Instruções de Uso

## Build e Execução

### Usando Docker Compose (Recomendado)

```bash
# Build e iniciar o container
docker-compose up -d --build

# Ver logs
docker-compose logs -f api

# Parar o container
docker-compose down

# Parar e remover volumes
docker-compose down -v
```

### Usando Docker diretamente

```bash
# Build da imagem
docker build -t investimentos-api:latest .

# Criar diretório para dados
mkdir -p ./data

# Executar container
docker run -d \
  --name investimentos-api \
  -p 8080:80 \
  -v $(pwd)/data:/app/data \
  -e ASPNETCORE_ENVIRONMENT=Production \
  investimentos-api:latest

# Ver logs
docker logs -f investimentos-api

# Parar container
docker stop investimentos-api

# Remover container
docker rm investimentos-api
```

## Testar a API

Após iniciar o container, a API estará disponível em:

- **Base URL**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/api/health

### Exemplos de Requisições

```bash
# Health Check
curl http://localhost:8080/api/health

# Login (para obter token JWT)
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "senha": "senha123"
  }'

# Simular Investimento (com bypass de desenvolvimento)
curl -X POST http://localhost:8080/api/simular-investimento \
  -H "Content-Type: application/json" \
  -H "X-Debug-Bypass: 1" \
  -d '{
    "clienteId": 1,
    "valor": 10000,
    "prazoMeses": 12
  }'

# Obter Recomendações de Produtos
curl http://localhost:8080/api/produtos/recomendacoes/1 \
  -H "X-Debug-Bypass: 1"
```

## Estrutura de Volumes

O container persiste o banco de dados SQLite no diretório `./data`:

```
desafio-investimentos/
├── data/                          # Volume montado
│   ├── investimentos.db           # Banco de dados SQLite
│   ├── investimentos.db-shm       # Shared memory file
│   └── investimentos.db-wal       # Write-Ahead Log
```

## Variáveis de Ambiente

| Variável                               | Descrição                | Valor Padrão                             |
|----------------------------------------|--------------------------|------------------------------------------|
| `ASPNETCORE_ENVIRONMENT`               | Ambiente da aplicação    | `Production`                             |
| `ASPNETCORE_URLS`                      | URLs de escuta           | `http://+:80`                            |
| `ConnectionStrings__DefaultConnection` | String de conexão SQLite | `Data Source=/app/data/investimentos.db` |

## Troubleshooting

### Banco de dados não persiste
Verifique se o diretório `./data` existe e tem permissões corretas:
```bash
mkdir -p ./data
chmod 755 ./data
```

### Porta 8080 já em uso
Altere a porta no docker-compose.yml:
```yaml
ports:
  - "8081:80"  # Use outra porta
```

### Ver logs detalhados
```bash
docker-compose logs -f api
```

### Acessar shell do container
```bash
docker exec -it investimentos-api /bin/bash
```

### Reiniciar banco de dados
```bash
docker-compose down -v
rm -rf ./data
docker-compose up -d --build
```
