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
echo iptables-persistent iptables-persistent/autosave_v4 boolean true | debconf-set-selections
echo iptables-persistent iptables-persistent/autosave_v6 boolean true | debconf-set-selections
apt-get install iptables-persistent -y
netfilter-persistent save