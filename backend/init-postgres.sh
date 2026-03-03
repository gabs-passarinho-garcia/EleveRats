#!/bin/bash
set -e

# Criando o banco de dados separado para o n8n no PostgreSQL usando o Superuser
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
	CREATE USER n8n_user WITH ENCRYPTED PASSWORD 'n8n_password';
	CREATE DATABASE n8n_db;
	GRANT ALL PRIVILEGES ON DATABASE n8n_db TO n8n_user;
EOSQL
