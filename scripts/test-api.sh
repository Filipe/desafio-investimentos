#!/bin/bash

# Script de teste da API de Investimentos

BASE_URL="http://localhost:5222/api"

echo "================================"
echo "Testando API de Investimentos"
echo "================================"
echo ""

# 1. Health Check
echo "1. Testando Health Check..."
curl -s "$BASE_URL/health" -w "\nStatus: %{http_code}\n\n"

# 2. Simulação de investimento CDB
echo "2. Simulando investimento em CDB..."
curl -s -X POST "$BASE_URL/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": 1,
    "valor": 10000.00,
    "prazoMeses": 12,
    "tipoProduto": "CDB"
  }' | jq '.' 2>/dev/null || echo "Erro ao processar resposta"
echo ""

# 3. Simulação de investimento Fundo
echo "3. Simulando investimento em Fundo..."
curl -s -X POST "$BASE_URL/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": 1,
    "valor": 5000.00,
    "prazoMeses": 6,
    "tipoProduto": "Fundo"
  }' | jq '.' 2>/dev/null || echo "Erro ao processar resposta"
echo ""

# 4. Listar todas as simulações
echo "4. Listando todas as simulações..."
curl -s "$BASE_URL/simulacoes" | jq '.' 2>/dev/null || echo "Erro ao processar resposta"
echo ""

# 5. Obter simulações por produto e dia
echo "5. Obtendo simulações agrupadas por produto e dia..."
curl -s "$BASE_URL/simulacoes/por-produto-dia" | jq '.' 2>/dev/null || echo "Erro ao processar resposta"
echo ""

# 6. Histórico de investimentos por cliente
echo "6. Obtendo histórico de investimentos do cliente 1..."
curl -s "$BASE_URL/investimentos/1" | jq '.' 2>/dev/null || echo "Erro ao processar resposta"
echo ""

# 7. Dados de telemetria
echo "7. Obtendo dados de telemetria..."
curl -s "$BASE_URL/telemetria" | jq '.' 2>/dev/null || echo "Erro ao processar resposta"
echo ""

echo "================================"
echo "Testes concluídos!"
echo "================================"
