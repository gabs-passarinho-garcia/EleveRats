#!/bin/bash

# Terminal color definitions for stylized output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

if [ -f ../.env ]; then
  set -a
  source ../.env
  set +a
else
  echo -e "${RED}❌ Arquivo .env não encontrado! Crie um antes de entrar em quadra.${NC}"
  exit 1
fi


# Function to send a success notification to Discord
send_notification() {
  if [ "$WEBHOOK_URL" != "COLE_SEU_WEBHOOK_AQUI" ]; then
    curl -H "Content-Type: application/json" \
         -d '{"content": "🏀 **SWISH!** A Nave-Mãe furou o bloqueio da Oracle e foi provisionada com sucesso! O garrafão é nosso, Gabs!"}' \
         "$WEBHOOK_URL" > /dev/null 2>&1
  fi
}

echo -e "${BLUE}=================================================${NC}"
echo -e "${YELLOW}🏀 Iniciando o cerco à Oracle - Modo Tiozão Ativado!${NC}"
echo -e "${BLUE}=================================================${NC}\n"

tofu init

# Initialize the attempt counter
ATTEMPT=1

while true; do
  # Print the current attempt with a timestamp
  echo -e "\n${BLUE}[$(date +'%H:%M:%S')] 🚀 Tentativa número: ${ATTEMPT}${NC}"
  
  tofu apply -auto-approve
  
  # Check if the last command (tofu apply) was successful
  if [ $? -eq 0 ]; then
    echo -e "\n${GREEN}=================================================${NC}"
    echo -e "${GREEN}🎯 SWISH! Cesta de 3 pontos no estouro do cronômetro!${NC}"
    echo -e "${GREEN}🛸 A Nave-Mãe está oficialmente em órbita!${NC}"
    echo -e "${GREEN}=================================================${NC}\n"
    
    # Trigger the notification
    send_notification

    # Executa o script de deploy completo agora que a máquina garantida
    echo -e "${BLUE}=================================================${NC}"
    echo -e "${YELLOW}🚀 Chamando o Feitiço de Deploy para configurar a Nave-Mãe...${NC}"
    echo -e "${BLUE}=================================================${NC}\n"
    
    cd ..
    bash deploy.sh
    break
  fi
  
  echo -e "\n${RED}❌ Toco do servidor! Sem vaga no garrafão da Oracle agora.${NC}"
  echo -e "${YELLOW}⏳ O Tiozão vai secar a careca, tomar uma água e tenta de novo em 60 segundos...${NC}"
  
  # 60-second visual countdown timer
  for i in {60..1}; do
    echo -ne "\rPróximo arremesso em: $i segundos... \033[0K"
    sleep 1
  done
  
  # Clear the countdown line before the next iteration
  echo -e "\r${YELLOW}Bora pro rebote!                                   ${NC}"
  
  ((ATTEMPT++))
done