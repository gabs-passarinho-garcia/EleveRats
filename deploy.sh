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
# 2. DESCONEXÃO SEGURA (SAFETY FIRST)
# ==========================================
# Tenta descobrir o IP antigo (antes do apply) para preparar a máquina para alterações (caso a VM seja destruída/substituída pelo Tofu)
OLD_IP=$SERVER_IP
if [ -z "$OLD_IP" ]; then
  cd terraform
  OLD_IP=$(tofu output -raw public_ip 2>/dev/null || echo "")
  cd ..
fi

# Define o SSH User (Padrão: ubuntu para imagens da Canonical na OCI)
SSH_USER=${SSH_USER:-ubuntu}

# Define a chave SSH para as conexões
if [ -z "$SSH_PRIVATE_KEY_PATH" ]; then
  SSH_KEY_ARG="-i ~/.ssh/id_oracle_nave_mae"
else
  SSH_KEY_ARG="-i $SSH_PRIVATE_KEY_PATH"
fi

if [ -n "$OLD_IP" ] && [ "$OLD_IP" != "No outputs found" ]; then
  echo "🛑 Preparando Nave-Mãe atual ($OLD_IP) para atualizações destrutivas..."
  # O '|| true' evita que o script quebre se a VM já estiver inacessível
  ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no -o ConnectTimeout=10 $SSH_USER@$OLD_IP "bash -s" << 'EOF' || true
    echo "Desligando containeres para evitar corrupção..."
    cd /mnt/dados/eleverats || true
    if [ -f "docker-compose.yml" ]; then
      docker compose down
    fi
    echo "Sincronizando I/O do disco..."
    sync
    echo "Desmontando volume paravirtualizado /mnt/dados..."
    cd /
    sudo umount -f /mnt/dados || echo "Volume não estava montado ou já havia sido solto."
EOF
else
  echo "🔍 Nenhum IP prévio detectado. Assumindo provisionamento do Zero."
fi

# ==========================================
# 3. PROVISIONAMENTO DA NUVEM (OPENTOFU)
# ==========================================
echo "🌩️ Evocando a Infraestrutura da Oracle Cloud (Tofu)..."
cd terraform

# Garante que os plugins tão baixados
tofu init -upgrade

# Applica as configurações (Cria VM, Cofres, Segredos) sem pedir confirmação
tofu apply -auto-approve

# Volta para a raiz
cd ..

# ==========================================
# 4. DEFINIÇÃO DE VARIÁVEIS ALVO ATUALIZADAS
# ==========================================

# Descobre o IP do servidor e Pega as referências do Vault do estado do Tofu
echo "🔍 Sincronizando referências e IPs do Tofu..."
cd terraform

if [ -z "$SERVER_IP" ]; then
  SERVER_IP=$(tofu output -raw public_ip 2>/dev/null || echo "")
  
  if [ -z "$SERVER_IP" ] || [ "$SERVER_IP" = "No outputs found" ]; then
    echo "❌ Erro: Não foi possível obter o IP do servidor após o apply."
    exit 1
  fi
fi

echo "🔐 Buscando OCIDs dos segredos no Tofu..."
DB_PASS_OCID=$(tofu output -raw db_password_secret_id)
DB_USER_OCID=$(tofu output -raw db_user_secret_id)
MINIO_USER_OCID=$(tofu output -raw minio_user_secret_id)
MINIO_PASS_OCID=$(tofu output -raw minio_password_secret_id)
REDIS_PASS_OCID=$(tofu output -raw redis_password_secret_id)
GRAFANA_PASS_OCID=$(tofu output -raw grafana_password_secret_id)
N8N_KEY_OCID=$(tofu output -raw n8n_encryption_key_secret_id)
CF_TOKEN_OCID=$(tofu output -raw cf_tunnel_token_secret_id)

cd ..

DEST_DIR="/mnt/dados/eleverats"
BRANCH=${BRANCH:-main}

echo "🎯 Novo Alvo Pós-Tofu: $SSH_USER@$SERVER_IP:$DEST_DIR"
echo "🌿 Branch Git: $BRANCH"

# ==========================================
# 5. ATUALIZAÇÃO DO CÓDIGO E CONTAINERS
# ==========================================
echo "📦 Baixando código da branch '$BRANCH' diretamente do GitHub para a Nave-Mãe..."

ssh $SSH_KEY_ARG -o StrictHostKeyChecking=no $SSH_USER@$SERVER_IP "bash -s" << EOF
  set -e
  
  # Garante pacote git instalado
  sudo apt-get update -yqq && sudo apt-get install -yqq git

  # Monta o volume se não estiver montado
  if ! mountpoint -q /mnt/dados; then
    echo "Volume /mnt/dados não está montado. Montando..."
    sudo mount -a || sudo mount /mnt/dados
  fi

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
  git checkout $BRANCH || git checkout -B $BRANCH origin/$BRANCH
  echo "🧹 Resetando e aplicando código fresco..."
  # Reseta qualquer mudança de arquivos, ignorando os arquivos do gitignore (ex: .env e volumes do docker)
  git reset --hard origin/$BRANCH

  echo "🔐 Atualizando segredos no arquivo .env a partir do OCI Vault..."
  
  # Busca os valores via OCI CLI remotamente usando sudo (perfil root tem os direitos do Dynamic Group)
  DB_PASSWORD=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${DB_PASS_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  DB_USER=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${DB_USER_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  MINIO_USER=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${MINIO_USER_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  MINIO_PASSWORD=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${MINIO_PASS_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  REDIS_PASSWORD=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${REDIS_PASS_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  GRAFANA_PASSWORD=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${GRAFANA_PASS_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  N8N_ENCRYPTION_KEY=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${N8N_KEY_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
  CF_TUNNEL_TOKEN=\$(sudo /root/bin/oci vault secret-bundle get --secret-id ${CF_TOKEN_OCID} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)

  # Função auxiliar para garantir o valor certo no .env
  update_env() {
    local key=\$1
    local val=\$2
    local file=".env"
    if grep -q "^\${key}=" "\$file"; then
      sed -i "s|^\${key}=.*|\${key}=\${val}|" "\$file"
    else
      echo "\${key}=\${val}" >> "\$file"
    fi
  }

  update_env "POSTGRES_USER" "\${DB_USER}"
  update_env "POSTGRES_PASSWORD" "\${DB_PASSWORD}"
  update_env "MINIO_ROOT_USER" "\${MINIO_USER}"
  update_env "MINIO_ROOT_PASSWORD" "\${MINIO_PASSWORD}"
  update_env "REDIS_PASSWORD" "\${REDIS_PASSWORD}"
  update_env "GRAFANA_SECURITY_ADMIN_PASSWORD" "\${GRAFANA_PASSWORD}"
  update_env "N8N_ENCRYPTION_KEY" "\${N8N_ENCRYPTION_KEY}"
  update_env "TUNNEL_TOKEN" "\${CF_TUNNEL_TOKEN}"

  echo "🐳 Reiniciando as engrenagens Docker na Nave-Mãe..."

  # Executa o compose usando os arquivos atualizados (com build forçado da nova Minimal API)
  echo "Invocando docker compose down..."
  sudo docker compose down

  echo "Invocando docker compose up -d --build..."
  sudo docker compose up -d --build
EOF

echo "🎉 Deploy finalizado com Glória! Soli Deo Gloria!"
