#!/bin/bash
set -e

echo "⚔️ Iniciando Feitiço de Deploy - EleveRats ⚔️"

# Carrega variáveis locais se existirem
[ -f .env ] && export $(grep -v '^#' .env | xargs)

# 1. Resolve o IP de acesso via Tailscale (o IP público OCI não é mais acessível diretamente)
SERVER_IP="${TAILSCALE_IP:?'TAILSCALE_IP não definido no .env. Abortando missão.'}"
cd terraform
tofu init -upgrade
cd ..

SSH_USER="ubuntu"
SSH_KEY_PATH="${SSH_PRIVATE_KEY_PATH:-$HOME/.ssh/id_oracle_nave_mae}"
SSH_KEY_ARG="-i $SSH_KEY_PATH"

echo "🛑 Desligando motores da Nave-Mãe ($SERVER_IP via Tailscale)..."
ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no -o ConnectTimeout=10 $SSH_USER@$SERVER_IP "bash -s" << 'EOF' || true
  cd /mnt/dados/eleverats && sudo docker compose down || true
  sync && sudo umount -f /mnt/dados || true
EOF

# 2. Aplica Infraestrutura (Aqui é onde a paciência com Ashburn entra)
echo "🌩️ Evocando a Infraestrutura na Oracle Cloud..."
cd terraform
tofu apply -auto-approve
cd ..

# 3. Atualização do Código e Containers
BRANCH=${BRANCH:-main}
DEST_DIR="/mnt/dados/eleverats"

echo "📦 Atualizando código na branch '$BRANCH' em $SERVER_IP..."

ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no $SSH_USER@$SERVER_IP "bash -s" << EOF
  set -e
  
  echo "⏳ Aguardando a Nave-Mãe terminar a sequência de inicialização (cloud-init)..."
  sudo cloud-init status --wait || true

  sudo apt-get update -yqq && sudo apt-get install -yqq git

  # Garante montagem do volume
  if ! mountpoint -q /mnt/dados; then
    sudo mount -a || sudo mount /mnt/dados
  fi

  # Limpeza drástica para deploy efêmero (O Banco e Env estão seguros fora da pasta)
  sudo rm -rf $DEST_DIR
  
  # Clona a branch do zero no destino
  git clone -b $BRANCH https://github.com/gabs-passarinho-garcia/EleveRats.git $DEST_DIR
  
  # Injecta o ambiente de volta no código clonado para o docker-compose fazer o parser das strings
  sudo cp /mnt/dados/eleverats-state/.env $DEST_DIR/.env
  
  cd $DEST_DIR
  sudo chown -R $SSH_USER:$SSH_USER $DEST_DIR

  echo "🐳 Reiniciando containers Docker..."
  sudo docker compose down || true
  sudo docker compose up -d --build
EOF

echo "🎉 Deploy finalizado com Glória! Soli Deo Gloria!"