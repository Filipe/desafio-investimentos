#!/bin/bash

# Script completo de teste de todos os endpoints da API de Investimentos

API_URL="http://localhost:5222/api"

echo "=========================================="
echo "🧪 TESTE COMPLETO DA API DE INVESTIMENTOS"
echo "=========================================="
echo ""
echo "📋 Requisitos testados:"
echo "   ✓ Simulação de investimentos"
echo "   ✓ Histórico de simulações"
echo "   ✓ Simulações por produto/dia"
echo "   ✓ Dados de telemetria"
echo "   ✓ Perfil de risco"
echo "   ✓ Produtos recomendados"
echo "   ✓ Histórico de investimentos"
echo "   ✓ Autenticação JWT"
echo ""

# ==========================================
# 1. HEALTH CHECK
# ==========================================
echo "=========================================="
echo "1️⃣  HEALTH CHECK"
echo "=========================================="
echo "GET /api/health"
echo ""
curl -s -w "HTTP Status: %{http_code}\n" "${API_URL}/health"
echo ""
echo ""

# ==========================================
# 2. AUTENTICAÇÃO JWT
# ==========================================
echo "=========================================="
echo "2️⃣  AUTENTICAÇÃO JWT"
echo "=========================================="
echo "POST /api/auth/login"
echo ""
LOGIN_RESPONSE=$(curl -s "${API_URL}/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"clienteId": 1}')

echo "$LOGIN_RESPONSE" | jq
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token')
echo ""
echo "🔑 Token obtido: ${TOKEN:0:60}..."
echo ""
echo ""

# ==========================================
# 3. PERFIL DE RISCO
# ==========================================
echo "=========================================="
echo "3️⃣  PERFIL DE RISCO"
echo "=========================================="
echo "GET /api/perfil-risco/1"
echo ""
PERFIL_RESPONSE=$(curl -s "${API_URL}/perfil-risco/1")
echo "$PERFIL_RESPONSE" | jq
PERFIL=$(echo "$PERFIL_RESPONSE" | jq -r '.nome')
PONTUACAO=$(echo "$PERFIL_RESPONSE" | jq -r '.pontuacao')
echo ""
echo "📊 Cliente possui perfil: $PERFIL (pontuação: $PONTUACAO)"
echo ""
echo ""

# ==========================================
# 4. PRODUTOS RECOMENDADOS
# ==========================================
echo "=========================================="
echo "4️⃣  PRODUTOS RECOMENDADOS"
echo "=========================================="
echo ""

echo "4.1. Produtos para perfil Conservador"
echo "GET /api/produtos-recomendados/Conservador"
echo "-------------------------------------------"
curl -s "${API_URL}/produtos-recomendados/Conservador" | jq
echo ""

echo "4.2. Produtos para perfil Moderado"
echo "GET /api/produtos-recomendados/Moderado"
echo "-------------------------------------------"
curl -s "${API_URL}/produtos-recomendados/Moderado" | jq
echo ""

echo "4.3. Produtos para perfil Agressivo"
echo "GET /api/produtos-recomendados/Agressivo"
echo "-------------------------------------------"
curl -s "${API_URL}/produtos-recomendados/Agressivo" | jq
echo ""
echo ""

# ==========================================
# 5. SIMULAÇÃO DE INVESTIMENTOS
# ==========================================
echo "=========================================="
echo "5️⃣  SIMULAÇÃO DE INVESTIMENTOS"
echo "=========================================="
echo ""

echo "5.1. Simulação em CDB (12 meses)"
echo "POST /api/simular-investimento"
echo "-------------------------------------------"
SIM1_RESPONSE=$(curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "clienteId": 1,
    "valor": 10000.00,
    "prazoMeses": 12,
    "tipoProduto": "CDB"
  }')
echo "$SIM1_RESPONSE" | jq
echo ""

echo "5.2. Simulação em Fundo (6 meses)"
echo "POST /api/simular-investimento"
echo "-------------------------------------------"
SIM2_RESPONSE=$(curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "clienteId": 1,
    "valor": 5000.00,
    "prazoMeses": 6,
    "tipoProduto": "Fundo"
  }')
echo "$SIM2_RESPONSE" | jq
echo ""

echo "5.3. Simulação em CDB (24 meses)"
echo "POST /api/simular-investimento"
echo "-------------------------------------------"
SIM3_RESPONSE=$(curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "clienteId": 1,
    "valor": 15000.00,
    "prazoMeses": 24,
    "tipoProduto": "CDB"
  }')
echo "$SIM3_RESPONSE" | jq
echo ""
echo ""

# ==========================================
# 6. HISTÓRICO DE SIMULAÇÕES
# ==========================================
echo "=========================================="
echo "6️⃣  HISTÓRICO DE SIMULAÇÕES"
echo "=========================================="
echo "GET /api/simulacoes"
echo ""
SIMULACOES=$(curl -s "${API_URL}/simulacoes" \
  -H "Authorization: Bearer $TOKEN")
echo "$SIMULACOES" | jq
TOTAL_SIMULACOES=$(echo "$SIMULACOES" | jq '. | length')
echo ""
echo "📈 Total de simulações: $TOTAL_SIMULACOES"
echo ""
echo ""

# ==========================================
# 7. SIMULAÇÕES POR PRODUTO E DIA
# ==========================================
echo "=========================================="
echo "7️⃣  SIMULAÇÕES POR PRODUTO E DIA"
echo "=========================================="
echo "GET /api/simulacoes/por-produto-dia"
echo ""
curl -s "${API_URL}/simulacoes/por-produto-dia" | jq
echo ""
echo ""

# ==========================================
# 8. HISTÓRICO DE INVESTIMENTOS POR CLIENTE
# ==========================================
echo "=========================================="
echo "8️⃣  HISTÓRICO DE INVESTIMENTOS"
echo "=========================================="
echo "GET /api/investimentos/1"
echo ""
INVESTIMENTOS=$(curl -s "${API_URL}/investimentos/1")
echo "$INVESTIMENTOS" | jq
TOTAL_INVESTIMENTOS=$(echo "$INVESTIMENTOS" | jq '. | length')
echo ""
echo "💰 Total de investimentos do cliente: $TOTAL_INVESTIMENTOS"
echo ""
echo ""

# ==========================================
# 9. DADOS DE TELEMETRIA
# ==========================================
echo "=========================================="
echo "9️⃣  DADOS DE TELEMETRIA"
echo "=========================================="
echo "GET /api/telemetria"
echo ""
TELEMETRIA=$(curl -s "${API_URL}/telemetria")
echo "$TELEMETRIA" | jq
echo ""
echo "📊 Resumo de telemetria:"
echo "$TELEMETRIA" | jq -r '.servicos[] | "   • \(.nome): \(.quantidadeChamadas) chamadas, média \(.mediaTempoRespostaMs)ms"'
echo ""
echo ""

# ==========================================
# 10. TESTE DE VALIDAÇÃO
# ==========================================
echo "=========================================="
echo "🔟 TESTES DE VALIDAÇÃO"
echo "=========================================="
echo ""

echo "10.1. Simulação com valor inválido (espera 400)"
echo "POST /api/simular-investimento (valor = 0)"
echo "-------------------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "clienteId": 1,
    "valor": 0,
    "prazoMeses": 12,
    "tipoProduto": "CDB"
  }' | jq
echo ""

echo "10.2. Acesso sem autenticação (espera 401)"
echo "POST /api/simular-investimento (sem token)"
echo "-------------------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": 1,
    "valor": 10000,
    "prazoMeses": 12,
    "tipoProduto": "CDB"
  }'
echo ""
echo ""

echo "10.3. Perfil de risco - cliente inexistente (espera 404)"
echo "GET /api/perfil-risco/999"
echo "-------------------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/perfil-risco/999" | jq
echo ""
echo ""

echo "=========================================="
echo "TODOS OS TESTES CONCLUÍDOS!"
echo "=========================================="