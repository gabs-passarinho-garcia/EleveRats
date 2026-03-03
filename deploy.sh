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
# 2. PROVISIONAMENTO DA NUVEM (TERRAFORM)
# ==========================================
echo "🌩️ Evocando a Infraestrutura da Oracle Cloud (Terraform)..."
cd terraform

# Garante que os plugins tão baixados
terraform init -upgrade

# Applica as configurações (Cria VM, Cofres, Segredos) sem pedir confirmação
terraform apply -auto-approve

# Volta para a raiz
cd ..

# ==========================================
# 3. DEFINIÇÃO DE VARIÁVEIS ALVO
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
BRANCH=${BRANCH:-main}

echo "🎯 Alvo: $SSH_USER@$SERVER_IP:$DEST_DIR"
echo "🌿 Branch Git: $BRANCH"

# ==========================================
# 4. ATUALIZAÇÃO DO CÓDIGO E CONTAINERS
# ==========================================
echo "📦 Baixando código da branch '$BRANCH' diretamente do GitHub para a Nave-Mãe..."

ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no $SSH_USER@$SERVER_IP "bash -s" << EOF
  set -e
  
  # Garante pacote git instalado
  sudo apt-get update -yqq && sudo apt-get install -yqq git

  # Entra na pasta do projeto (A base já deve ter sido criada pelo Cloud-Init com o .env dentro)
  cd $DEST_DIR
  
  # Garante permissões pro ubuntu poder recriar o repo
  sudo chown -R $SSH_USER:$SSH_USER $DEST_DIR

  # Configura repositório Git se não existir
  if [ ! -d ".git" ]; then
    echo "Inicializando repositório Git do zero..."
    git init
    git remote add origin https://github.com/gabs-passarinho-garcia/EleveRats.git
  fi

  echo "🔄 Atualizando referências do GitHub..."
  git fetch origin

  # Força o branch atual (Cria se não existir, reseta apontando para o origin)
  git checkout $BRANCH || git checkout -b $BRANCH origin/$BRANCH
  echo "🧹 Resetando e aplicando código fresco..."
  # Reseta qualquer mudança de arquivos, ignorando os arquivos do gitignore (ex: .env e volumes do docker)
  git reset --hard origin/$BRANCH

  echo "🐳 Reiniciando as engrenagens Docker na Nave-Mãe..."

  # Executa o compose usando os arquivos atualizados (com build forçado da nova Minimal API)
  echo "Invocando docker compose down..."
  sudo docker compose down

  echo "Invocando docker compose up -d --build..."
  sudo docker compose up -d --build
EOF

echo "🎉 Deploy finalizado com Glória! Soli Deo Gloria!"
