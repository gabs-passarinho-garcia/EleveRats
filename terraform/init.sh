#!/bin/bash
# Atualiza o sistema
apt-get update && apt-get upgrade -y

# ==========================================
# 1. CONFIGURAÇÃO DO DISCO EXTRA (130GB)
# ==========================================
DISCO="/dev/sdb"
PONTO_MONTAGEM="/mnt/dados"

# Dá um tempinho pro OCI plugar o disco paravirtualizado na VM
sleep 10

# Verifica se o disco realmente foi anexado
if [ -b "$DISCO" ]; then
  # Checa se o disco já tem um sistema de arquivos (pra não formatar se você rodar o script de novo)
  if ! blkid "$DISCO" > /dev/null; then
    echo "Disco cru detectado. Formatando $DISCO em ext4..."
    mkfs.ext4 -F "$DISCO"
  fi

  # Cria o diretório base e monta
  mkdir -p "$PONTO_MONTAGEM"
  mount "$DISCO" "$PONTO_MONTAGEM"

  # Pega o UUID do disco para ancorar no fstab com segurança máxima
  UUID=$(blkid -s UUID -o value "$DISCO")
  
  # Adiciona no fstab (se já não estiver lá) para montar automático no boot
  if ! grep -q "$UUID" /etc/fstab; then
    echo "UUID=$UUID  $PONTO_MONTAGEM  ext4  defaults,_netdev,nofail  0  2" >> /etc/fstab
  fi
  
  # Garante que o usuário ubuntu consiga ler e escrever lá sem precisar de sudo toda hora
  chown -R ubuntu:ubuntu "$PONTO_MONTAGEM"
fi

# ==========================================
# 2. INSTALAÇÃO DO DOCKER
# ==========================================
# Instala dependências e Docker (perfeito para ARM/aarch64)
apt-get install -y curl git apt-transport-https ca-certificates gnupg-agent software-properties-common
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
usermod -aG docker ubuntu

# ==========================================
# 3. FIREWALL INTERNO (IPTABLES)
# ==========================================
# Firewall Interno (iptables) - APENAS SSH
# Como você vai usar Cloudflare Tunnel, não abrimos 80, 443 ou 3000 aqui.
iptables -I INPUT 6 -p tcp --dport 22 -j ACCEPT
iptables -A INPUT -p tcp --dport 80 -j DROP
iptables -A INPUT -p tcp --dport 443 -j DROP
iptables -A INPUT -p tcp --dport 3000 -j DROP

# Torna as regras persistentes
# ==========================================
# 4. SETUP DO OCI CLI E CRIAÇÃO DO .ENV DA APLICAÇÃO
# ==========================================
echo "Configurando OCI CLI via Instance Principal..."
apt-get install -y python3-pip
bash -c "$(curl -L https://raw.githubusercontent.com/oracle/oci-cli/master/scripts/install/install.sh)" -s --accept-all-defaults

# Prepara o ambiente pro OCI CLI funcionar com a identidade da Instância (Dynamic Group)
export OCI_CLI_AUTH="instance_principal"

# Busca a senha do banco gerada via Terraform que tá guardada no Vault
echo "Buscando secret do banco de dados no OCI Vault..."
# $${db_password_secret_id} escapa o template so o Terraform injeta a variavel aqui certinho.
DB_PASSWORD=$(/root/bin/oci vault secret-bundle get --secret-id ${db_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
DB_USER=$(/root/bin/oci vault secret-bundle get --secret-id ${db_user_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
MINIO_USER=$(/root/bin/oci vault secret-bundle get --secret-id ${minio_user_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
MINIO_PASSWORD=$(/root/bin/oci vault secret-bundle get --secret-id ${minio_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
REDIS_PASSWORD=$(/root/bin/oci vault secret-bundle get --secret-id ${redis_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
GRAFANA_PASSWORD=$(/root/bin/oci vault secret-bundle get --secret-id ${grafana_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
N8N_ENCRYPTION_KEY=$(/root/bin/oci vault secret-bundle get --secret-id ${n8n_encryption_key_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
CF_TUNNEL_TOKEN=$(/root/bin/oci vault secret-bundle get --secret-id ${cf_tunnel_token_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)

# Prepara o diretório e o arquivo .env
APP_DIR="/mnt/dados/eleverats"
mkdir -p $APP_DIR

# Escreve o .env com as senhas extraídas do Vault
cat <<EOF > $APP_DIR/.env
DB_USER=$DB_USER
DB_PASSWORD=$DB_PASSWORD
MINIO_USER=$MINIO_USER
MINIO_PASSWORD=$MINIO_PASSWORD
REDIS_PASSWORD=$REDIS_PASSWORD
GRAFANA_PASSWORD=$GRAFANA_PASSWORD
N8N_ENCRYPTION_KEY=$N8N_ENCRYPTION_KEY
CF_TUNNEL_TOKEN=$CF_TUNNEL_TOKEN
EOF

chown -R ubuntu:ubuntu $APP_DIR
chmod 600 $APP_DIR/.env

echo "Init script concluído com sucesso!"