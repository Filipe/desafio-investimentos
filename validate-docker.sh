#!/bin/bash

# Script de validação para Docker build e execução
# Execute este script para validar a configuração Docker

set -e

echo "🐳 Validando configuração Docker..."
echo ""

# 1. Verificar se Docker está instalado
if ! command -v docker &> /dev/null; then
    echo "❌ Docker não está instalado. Instale Docker Desktop: https://www.docker.com/products/docker-desktop"
    exit 1
fi

echo "✅ Docker encontrado: $(docker --version)"

# 2. Verificar se Docker Compose está disponível
if ! command -v docker-compose &> /dev/null; then
    echo "⚠️  docker-compose não encontrado, tentando 'docker compose'..."
    if ! docker compose version &> /dev/null; then
        echo "❌ Docker Compose não está disponível"
        exit 1
    fi
    COMPOSE_CMD="docker compose"
else
    COMPOSE_CMD="docker-compose"
    echo "✅ Docker Compose encontrado: $(docker-compose --version)"
fi

echo ""
echo "📦 Iniciando build da imagem..."
docker build -t investimentos-api:latest .

if [ $? -eq 0 ]; then
    echo "✅ Build concluído com sucesso!"
else
    echo "❌ Erro no build da imagem"
    exit 1
fi

echo ""
echo "🚀 Criando diretório de dados..."
mkdir -p ./data

echo ""
echo "🎯 Iniciando container..."
$COMPOSE_CMD up -d

if [ $? -eq 0 ]; then
    echo "✅ Container iniciado com sucesso!"
else
    echo "❌ Erro ao iniciar container"
    exit 1
fi

echo ""
echo "⏳ Aguardando API inicializar (10 segundos)..."
sleep 10

echo ""
echo "🏥 Testando health check..."
HEALTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/health)

if [ "$HEALTH_RESPONSE" = "200" ]; then
    echo "✅ Health check passou! API está respondendo na porta 8080"
else
    echo "❌ Health check falhou (HTTP $HEALTH_RESPONSE)"
    echo "📋 Logs do container:"
    $COMPOSE_CMD logs --tail=50 api
    exit 1
fi

echo ""
echo "📊 Status dos containers:"
$COMPOSE_CMD ps

echo ""
echo "✨ Validação concluída com sucesso!"
echo ""
echo "🌐 A API está disponível em:"
echo "   - Base URL: http://localhost:8080"
echo "   - Swagger:  http://localhost:8080/swagger"
echo "   - Health:   http://localhost:8080/api/health"
echo ""
echo "📝 Para ver logs: $COMPOSE_CMD logs -f api"
echo "🛑 Para parar:    $COMPOSE_CMD down"
