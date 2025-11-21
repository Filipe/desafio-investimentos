#!/bin/bash

# Script para limpar arquivos temporários antes de exportar o projeto

# Ir para o diretório raiz do projeto
cd "$(dirname "$0")/.." || exit 1

echo "🧹 Limpando arquivos temporários do projeto..."
echo ""

# 1. Limpar pastas bin e obj (builds)
echo "📦 Removendo pastas bin/ e obj/..."
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null
echo "✅ Pastas bin/ e obj/ removidas"

# 2. Limpar bancos de dados SQLite (exceto o do volume Docker)
echo ""
echo "🗄️  Removendo bancos de dados SQLite de desenvolvimento..."
find ./src -name "*.db" -exec rm -f {} + 2>/dev/null
find ./src -name "*.db-shm" -exec rm -f {} + 2>/dev/null
find ./src -name "*.db-wal" -exec rm -f {} + 2>/dev/null
echo "✅ Bancos de dados removidos (volume Docker preservado)"

# 3. Limpar arquivos de log
echo ""
echo "📝 Removendo arquivos de log..."
find . -name "*.log" -exec rm -f {} + 2>/dev/null
echo "✅ Logs removidos"

# 4. Limpar pasta .vs (Visual Studio)
echo ""
echo "🔧 Removendo pasta .vs do Visual Studio..."
find . -type d -name ".vs" -exec rm -rf {} + 2>/dev/null
echo "✅ Pasta .vs removida"

# 5. Limpar caches do Rider (JetBrains)
echo ""
echo "🔧 Removendo pastas .idea do Rider..."
find . -type d -name ".idea" -exec rm -rf {} + 2>/dev/null
echo "✅ Pastas .idea removidas"

# 6. Limpar arquivos de usuário
echo ""
echo "👤 Removendo arquivos de configuração de usuário..."
find . -name "*.user" -exec rm -f {} + 2>/dev/null
find . -name "*.suo" -exec rm -f {} + 2>/dev/null
echo "✅ Arquivos de usuário removidos"

# 7. Limpar pasta de data protection keys
echo ""
echo "🔑 Removendo chaves de proteção de dados temporárias..."
rm -rf ~/.aspnet/DataProtection-Keys 2>/dev/null
echo "✅ Chaves removidas"

# 8. Limpar imagens Docker antigas (opcional)
echo ""
read -p "❓ Deseja limpar imagens Docker antigas? (s/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Ss]$ ]]; then
    echo "🐳 Removendo imagens Docker do projeto..."
    docker rmi desafio-investimentos-api 2>/dev/null || true
    docker image prune -f 2>/dev/null || true
    echo "✅ Imagens Docker limpas"
else
    echo "⏭️  Imagens Docker preservadas"
fi

echo ""
echo "✨ Limpeza concluída!"
echo ""
echo "📊 Tamanho do projeto:"
du -sh . 2>/dev/null || echo "Não foi possível calcular o tamanho"
echo ""
echo "📦 O projeto está pronto para ser exportado!"
echo ""
