#!/bin/bash

# Script de teste para endpoints de perfil de risco e recomendação de produtos
API_URL="http://localhost:5222/api"

echo "==================================="
echo "Teste de Perfil de Risco e Recomendações"
echo "==================================="
echo ""

# Teste 1: Obter perfil de risco do cliente
echo "1. GET /api/perfil-risco/1 - Obter perfil do cliente João"
echo "-----------------------------------"
curl -s "${API_URL}/perfil-risco/1" | jq
echo ""
echo ""

# Teste 2: Tentar obter perfil de cliente inexistente
echo "2. GET /api/perfil-risco/999 - Cliente inexistente (espera 404)"
echo "-----------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/perfil-risco/999" | jq
echo ""
echo ""

# Teste 3: Produtos recomendados para perfil Conservador
echo "3. GET /api/produtos-recomendados/Conservador"
echo "-----------------------------------"
curl -s "${API_URL}/produtos-recomendados/Conservador" | jq
echo ""
echo ""

# Teste 4: Produtos recomendados para perfil Moderado
echo "4. GET /api/produtos-recomendados/Moderado"
echo "-----------------------------------"
curl -s "${API_URL}/produtos-recomendados/Moderado" | jq
echo ""
echo ""

# Teste 5: Produtos recomendados para perfil Agressivo
echo "5. GET /api/produtos-recomendados/Agressivo"
echo "-----------------------------------"
curl -s "${API_URL}/produtos-recomendados/Agressivo" | jq
echo ""
echo ""

# Teste 6: Perfil inválido (espera 400)
echo "6. GET /api/produtos-recomendados/Invalido (espera 400)"
echo "-----------------------------------"
curl -s -w "\nHTTP Status: %{http_code}\n" "${API_URL}/produtos-recomendados/Invalido" | jq
echo ""
echo ""

echo "==================================="
echo "Testes concluídos!"
echo "==================================="
