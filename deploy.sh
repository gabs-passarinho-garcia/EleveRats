#!/bin/bash
set -e

echo "⚔️ Iniciando Feitiço de Deploy - EleveRats ⚔️"

# Carrega variáveis locais se existirem
[ -f .env ] && export $(grep -v '^#' .env | xargs)

# 1. Busca IP atual para limpeza preventiva
cd terraform
tofu init -upgrade
OLD_IP=$(tofu output -raw public_ip 2>/dev/null || echo "")
cd ..

SSH_USER="ubuntu"
SSH_KEY_ARG="-i ~/.ssh/id_oracle_nave_mae"

if [ -n "$OLD_IP" ] && [ "$OLD_IP" != "No outputs found" ]; then
  echo "🛑 Desligando motores da Nave-Mãe antiga ($OLD_IP)..."
  ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no -o ConnectTimeout=10 $SSH_USER@$OLD_IP "bash -s" << 'EOF' || true
    cd /mnt/dados/eleverats && sudo docker compose down || true
    sync && sudo umount -f /mnt/dados || true
EOF
fi

# 2. Aplica Infraestrutura (Aqui é onde a paciência com Ashburn entra)
echo "🌩️ Evocando a Infraestrutura na Oracle Cloud..."
cd terraform
tofu apply -auto-approve
SERVER_IP=$(tofu output -raw public_ip)
cd ..

# 3. Atualização do Código e Containers
BRANCH=${BRANCH:-main}
DEST_DIR="/mnt/dados/eleverats"

echo "📦 Atualizando código na branch '$BRANCH' em $SERVER_IP..."

ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no $SSH_USER@$SERVER_IP "bash -s" << EOF
  set -e
  sudo apt-get update -yqq && sudo apt-get install -yqq git

  # Garante montagem do volume
  if ! mountpoint -q /mnt/dados; then
    sudo mount -a || sudo mount /mnt/dados
  fi

  cd $DEST_DIR
  sudo chown -R $SSH_USER:$SSH_USER $DEST_DIR

  if [ ! -d ".git" ]; then
    git init
    git remote add origin https://github.com/gabs-passarinho-garcia/EleveRats.git
  fi

  git fetch origin
  # -B: Força a branch local a espelhar a origin, não importa o que aconteça
  git checkout -B $BRANCH origin/$BRANCH
  git reset --hard origin/$BRANCH

  echo "🐳 Reiniciando containers Docker..."
  sudo docker compose down || true
  sudo docker compose up -d --build
EOF

echo "🎉 Deploy finalizado com Glória! Soli Deo Gloria!"