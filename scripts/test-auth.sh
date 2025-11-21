#!/bin/bash

# Script de teste para autenticação JWT
API_URL="http://localhost:5222/api"

echo "==================================="
echo "Teste de Autenticação JWT"
echo "==================================="
echo ""

# Teste 1: Login com cliente válido
echo "1. POST /api/auth/login - Login com cliente 1"
echo "-----------------------------------"
LOGIN_RESPONSE=$(curl -s "${API_URL}/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 1}')

echo "$LOGIN_RESPONSE" | jq
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token')
echo ""
echo "Token obtido: ${TOKEN:0:50}..."
echo ""

# Teste 2: Tentar acessar endpoint protegido SEM token (deve retornar 401)
echo "2. POST /api/simular-investimento SEM token (espera 401)"
echo "-----------------------------------"
RESPONSE=$(curl -s -w "\n%{http_code}" "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{"clienteId":1,"valor":10000,"prazoMeses":12,"tipoProduto":"CDB"}')
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')
echo "$BODY" | jq -e . 2>/dev/null || echo "$BODY"
echo "HTTP Status: $HTTP_CODE"
echo ""
echo ""

# Teste 3: Acessar endpoint protegido COM token válido
echo "3. POST /api/simular-investimento COM token válido"
echo "-----------------------------------"
curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"clienteId":1,"valor":10000,"prazoMeses":12,"tipoProduto":"CDB"}' | jq
echo ""

# Teste 4: GET /simulacoes COM token válido
echo "4. GET /api/simulacoes COM token válido"
echo "-----------------------------------"
curl -s "${API_URL}/simulacoes" \
  -H "Authorization: Bearer $TOKEN" | jq '. | length' | xargs -I {} echo "Total de simulações: {}"
echo ""

# Teste 5: Acessar com Development Bypass (X-Debug-Bypass: 1)
echo "5. POST /api/simular-investimento COM X-Debug-Bypass: 1"
echo "-----------------------------------"
curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -H "X-Debug-Bypass: 1" \
  -d '{"clienteId":1,"valor":5000,"prazoMeses":6,"tipoProduto":"Fundo"}' | jq
echo ""

# Teste 6: Login com cliente inexistente (deve retornar 404)
echo "6. POST /api/auth/login - Cliente inexistente (espera 404)"
echo "-----------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 999}' | jq
echo ""

# Teste 7: Endpoints públicos (não requerem autenticação)
echo "7. GET /api/health - Endpoint público"
echo "-----------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/health"
echo ""
echo ""

echo "8. GET /api/perfil-risco/1 - Endpoint público"
echo "-----------------------------------"
curl -s "${API_URL}/perfil-risco/1" | jq
echo ""

echo "==================================="
echo "Testes concluídos!"
echo "==================================="
