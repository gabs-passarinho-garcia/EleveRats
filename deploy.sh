#!/bin/bash
set -e

echo "⚔️ Iniciando Feitiço de Deploy - EleveRats ⚔️"

# ==========================================
# 1. PREPARAÇÃO DO AMBIENTE
# ==========================================

# Se o .env existir localmente, carrega as variáveis dele
if [ -f .env ]; then
  echo "📜 Encotrado arquivo .env, carregando feitiços secretos..."
  export $(grep -v '^#' .env | xargs)
fi

# ==========================================
# 2. DEFINIÇÃO DE VARIÁVEIS ALVO
# ==========================================

# Descobre o IP do servidor (via variável de ambiente ou Terraform)
if [ -z "$SERVER_IP" ]; then
  echo "🔍 SERVER_IP não fornecido. Tentando descobrir via Terraform..."
  cd terraform
  SERVER_IP=$(terraform output -raw public_ip 2>/dev/null || echo "")
  cd ..
  
  if [ -z "$SERVER_IP" ] || [ "$SERVER_IP" = "No outputs found" ]; then
    echo "❌ Erro: Não foi possível obter o IP do servidor. Defina SERVER_IP no .env ou rode o terraform apply."
    exit 1
  fi
fi

# Define o SSH User (Padrão: ubuntu para imagens da Canonical na OCI)
SSH_USER=${SSH_USER:-ubuntu}

# Define a chave SSH
if [ -z "$SSH_PRIVATE_KEY_PATH" ]; then
  # Se não tiver no .env, assume o caminho padrão que criamos no projeto
  SSH_KEY_ARG="-i ~/.ssh/id_oracle_nave_mae"
else
  SSH_KEY_ARG="-i $SSH_PRIVATE_KEY_PATH"
fi

DEST_DIR="/mnt/dados/eleverats"

echo "🎯 Alvo: $SSH_USER@$SERVER_IP:$DEST_DIR"

# ==========================================
# 3. SINCRONIZAÇÃO (RSYNC)
# ==========================================
echo "📦 Transferindo arquivos para a Nave-Mãe via rsync..."

# Vamos garantir que a pasta destino exista e tenha permissão
ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no $SSH_USER@$SERVER_IP "sudo mkdir -p $DEST_DIR && sudo chown -R $SSH_USER:$SSH_USER $DEST_DIR"

# Rsync: Copia tudo, ignorando lixo
rsync -avz --delete \
  -e "ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no" \
  --exclude '.git' \
  --exclude 'node_modules' \
  --exclude 'bin' \
  --exclude 'obj' \
  --exclude '.terraform' \
  --exclude '*.tfstate*' \
  --exclude 'data' \
  --exclude 'test' \
  ./ $SSH_USER@$SERVER_IP:$DEST_DIR/

echo "✅ Arquivos sincronizados."

# ==========================================
# 4. SUBINDO OS CONTAINERS COM MAGIA
# ==========================================
echo "🐳 Reiniciando as engrenagens Docker na Nave-Mãe..."

ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no $SSH_USER@$SERVER_IP << 'EOF'
  cd /mnt/dados/eleverats
  
  # Garante permissões corretas
  sudo chown -R ubuntu:ubuntu /mnt/dados/eleverats

  # Executa o compose usando os arquivos atualizados (com build forçado da nova Minimal API)
  echo "Invocando docker compose down..."
  sudo docker compose down

  echo "Invocando docker compose up -d --build..."
  sudo docker compose up -d --build
EOF

echo "🎉 Deploy finalizado com Glória! Soli Deo Gloria!"
