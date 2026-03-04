#!/bin/bash
set -e

# Atualiza o sistema
apt-get update && apt-get upgrade -y

# ==========================================
# 1. CONFIGURAÇÃO DO DISCO EXTRA (130GB)
# ==========================================
# Dá um tempinho pro OCI plugar o disco paravirtualizado na VM
sleep 15

# Busca o disco pelo link estável da Oracle. Se não existir (ou o udev do Ubuntu 24 falhar), foca no /dev/sdb
DISCO=$(readlink -f /dev/oracle_vvb/orclvd* 2>/dev/null || true)
if [ -z "$DISCO" ] || [ "$DISCO" = "/dev/oracle_vvb/orclvd*" ]; then
  DISCO="/dev/sdb"
fi
PONTO_MONTAGEM="/mnt/dados"

# Verifica se o disco realmente foi anexado
if [ -n "$DISCO" ] && [ -b "$DISCO" ]; then
  # Só formata se o disco estiver "cru" (idempotência)
  if ! blkid "$DISCO" > /dev/null; then
    echo "Disco cru detectado. Formatando $DISCO em ext4..."
    mkfs.ext4 -F "$DISCO"
  fi

  mkdir -p "$PONTO_MONTAGEM"
  if ! mountpoint -q "$PONTO_MONTAGEM"; then
    mount "$DISCO" "$PONTO_MONTAGEM"
  fi

  # Pega o UUID para o fstab ser à prova de falhas
  UUID=$(blkid -s UUID -o value "$DISCO")
  
  if ! grep -q "$UUID" /etc/fstab; then
    # _netdev: espera a rede subir | nofail: não trava o boot se o disco sumir
    echo "UUID=$UUID  $PONTO_MONTAGEM  ext4  defaults,_netdev,nofail  0  2" >> /etc/fstab
  fi
  
  chown -R ubuntu:ubuntu "$PONTO_MONTAGEM"
fi

# ==========================================
# 2. INSTALAÇÃO DO DOCKER
# ==========================================
# Só instala se não houver o executável do docker disponível
if ! command -v docker &> /dev/null; then
  apt-get install -y curl git apt-transport-https ca-certificates gnupg-agent software-properties-common
  curl -fsSL https://get.docker.com -o get-docker.sh
  sh get-docker.sh
  usermod -aG docker ubuntu
fi

# ==========================================
# 3. FIREWALL INTERNO (IPTABLES)
# ==========================================
# Bloqueia tudo via túnel, deixa apenas SSH
iptables -I INPUT 6 -p tcp --dport 22 -j ACCEPT
iptables -A INPUT -p tcp --dport 80 -j DROP
iptables -A INPUT -p tcp --dport 443 -j DROP
iptables -A INPUT -p tcp --dport 3000 -j DROP

# Instala persistência para as regras sobreviverem ao reboot
DEBIAN_FRONTEND=noninteractive apt-get install -y iptables-persistent
netfilter-persistent save

# ==========================================
# 4. SETUP DO OCI CLI E SECRETS (VAULT)
# ==========================================
apt-get install -y python3-pip
# Instalação silenciosa do OCI CLI só se não estiver instalado
if ! command -v /root/bin/oci &> /dev/null; then
  bash -c "$(curl -L https://raw.githubusercontent.com/oracle/oci-cli/master/scripts/install/install.sh)" -s --accept-all-defaults
fi

# Garante que o OCI CLI está no PATH para o root
export PATH=$PATH:/root/bin
export OCI_CLI_AUTH="instance_principal"

# Pequeno sleep para garantir que a Identity da Instância propagou no IAM da Oracle
sleep 10

echo "Buscando segredos no Vault..."
# Injeção via Terraform Template (escapando chaves se necessário)
DB_PASSWORD=$(/root/bin/oci secrets secret-bundle get --secret-id ${db_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
DB_USER=$(/root/bin/oci secrets secret-bundle get --secret-id ${db_user_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
MINIO_USER=$(/root/bin/oci secrets secret-bundle get --secret-id ${minio_user_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
MINIO_PASSWORD=$(/root/bin/oci secrets secret-bundle get --secret-id ${minio_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
REDIS_PASSWORD=$(/root/bin/oci secrets secret-bundle get --secret-id ${redis_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
GRAFANA_PASSWORD=$(/root/bin/oci secrets secret-bundle get --secret-id ${grafana_password_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
N8N_ENCRYPTION_KEY=$(/root/bin/oci secrets secret-bundle get --secret-id ${n8n_encryption_key_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)
CF_TUNNEL_TOKEN=$(/root/bin/oci secrets secret-bundle get --secret-id ${cf_tunnel_token_secret_id} --query 'data."secret-bundle-content".content' --raw-output | base64 --decode)

APP_DIR="/mnt/dados/eleverats"
mkdir -p "$APP_DIR"

cat <<EOF > "$APP_DIR/.env"
DB_USER=$DB_USER
DB_PASSWORD=$DB_PASSWORD
MINIO_USER=$MINIO_USER
MINIO_PASSWORD=$MINIO_PASSWORD
REDIS_PASSWORD=$REDIS_PASSWORD
GRAFANA_PASSWORD=$GRAFANA_PASSWORD
N8N_ENCRYPTION_KEY=$N8N_ENCRYPTION_KEY
CF_TUNNEL_TOKEN=$CF_TUNNEL_TOKEN
EOF

chown -R ubuntu:ubuntu "$APP_DIR"
chmod 600 "$APP_DIR/.env"

echo "✅ Nave-Mãe inicializada com sucesso! Soli Deo Gloria!"