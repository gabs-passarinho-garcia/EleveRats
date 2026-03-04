#!/bin/bash
set -e

# 1. Criando o usuário e o banco de dados (no banco principal/default)
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
	CREATE USER n8n_user WITH ENCRYPTED PASSWORD 'n8n_password';
	CREATE DATABASE n8n_db;
	GRANT ALL PRIVILEGES ON DATABASE n8n_db TO n8n_user;
EOSQL

# 2. O Remédio para o Postgres 15+:
# Conectamos especificamente no n8n_db para dar o controle do schema public
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "n8n_db" <<-EOSQL
	GRANT ALL ON SCHEMA public TO n8n_user;
	ALTER SCHEMA public OWNER TO n8n_user;
EOSQL