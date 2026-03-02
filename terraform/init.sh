#!/bin/bash
# Atualiza o sistema
apt-get update && apt-get upgrade -y

# Instala dependências e Docker (perfeito para ARM/aarch64)
apt-get install -y curl git apt-transport-https ca-certificates gnupg-agent software-properties-common
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
usermod -aG docker ubuntu

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
