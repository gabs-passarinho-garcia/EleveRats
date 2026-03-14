#!/bin/bash
# Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as
# published by the Free Software Foundation, either version 3 of the
# License, or (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

# Criando SQL das migrations
echo "📜 Forjando o Pergaminho de Migrations (SQL Idempotente)..."
dotnet ef migrations script --project backend --idempotent -o /tmp/apply_migrations.sql

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

echo "🚀 Enviando o Pergaminho para a Nave-Mãe..."
scp $SSH_KEY_ARG -o StrictHostKeyChecking=no /tmp/apply_migrations.sql $SSH_USER@$SERVER_IP:/tmp/apply_migrations.sql

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

  # Garante que pgbouncer.ini está lá (já vem no git clone, mas é bom validar)
  [ -f $DEST_DIR/pgbouncer/pgbouncer.ini ] || echo "⚠️ pgbouncer.ini ausente!"

  cd $DEST_DIR
  sudo chown -R $SSH_USER:$SSH_USER $DEST_DIR

  echo "🐳 Reiniciando containers Docker..."
  sudo docker compose down || true
  sudo docker compose up -d --build

  echo "⏳ Aguardando os motores do PostgreSQL aquecerem (10s)..."
  sleep 10 # Essencial: O banco precisa estar aceitando conexões

  echo "🧙‍♂️ Conjurando as Migrations no Banco de Dados..."
  source $DEST_DIR/.env # Carrega o DB_USER e DB_PASSWORD

  sudo docker exec -i -e PGPASSWORD="\$DB_PASSWORD" eleverats-db-1 psql -U "\$DB_USER" -d eleverats_db < /tmp/apply_migrations.sql

  # Limpa os rastros
  rm /tmp/apply_migrations.sql
EOF

echo "🧹 Varrendo o vestiário local..."
rm /tmp/apply_migrations.sql

echo "🎉 Deploy finalizado com Glória! Soli Deo Gloria!"
