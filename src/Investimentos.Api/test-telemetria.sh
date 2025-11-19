#!/bin/bash

# Script de teste para endpoint de telemetria
API_URL="http://localhost:5222/api"

echo "==================================="
echo "Teste de Telemetria"
echo "==================================="
echo ""

echo "Gerando dados de telemetria com várias chamadas..."
echo ""

# Fazer várias chamadas para gerar dados de telemetria
echo "1. Health checks..."
for i in {1..5}; do
  curl -s "${API_URL}/health" > /dev/null
done

echo "2. Simulações de investimento..."
curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{"clienteId":1,"valor":10000,"prazoMeses":12,"tipoProduto":"CDB"}' > /dev/null

curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{"clienteId":1,"valor":5000,"prazoMeses":6,"tipoProduto":"Fundo"}' > /dev/null

curl -s "${API_URL}/simular-investimento" \
  -H "Content-Type: application/json" \
  -d '{"clienteId":1,"valor":15000,"prazoMeses":18,"tipoProduto":"CDB"}' > /dev/null

echo "3. Consultas de perfil de risco..."
for i in {1..3}; do
  curl -s "${API_URL}/perfil-risco/1" > /dev/null
done

echo "4. Produtos recomendados..."
curl -s "${API_URL}/produtos-recomendados/Conservador" > /dev/null
curl -s "${API_URL}/produtos-recomendados/Moderado" > /dev/null
curl -s "${API_URL}/produtos-recomendados/Agressivo" > /dev/null

echo "5. Listagens..."
curl -s "${API_URL}/simulacoes" > /dev/null
curl -s "${API_URL}/simulacoes/por-produto-dia" > /dev/null

echo ""
echo "Dados de telemetria gerados!"
echo ""
echo "==================================="
echo "GET /api/telemetria"
echo "==================================="
curl -s "${API_URL}/telemetria" | jq
echo ""

echo "==================================="
echo "Teste concluído!"
echo "==================================="
