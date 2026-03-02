#!/bin/bash
echo "Iniciando o cerco à Oracle..."
tofu init
while true; do
  tofu apply -auto-approve
  if [ $? -eq 0 ]; then
    echo "Cesta de 3 pontos no estouro do cronômetro! A Nave-Mãe nasceu!"
    break
  fi
  echo "Sem vaga ainda. O Tiozão vai tomar uma água e tenta de novo em 60 segundos..."
  sleep 60
done