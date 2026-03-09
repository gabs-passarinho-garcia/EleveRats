#!/bin/bash
# Copyright (C) 2026 Gabriel Passarinho Garcia and EleveRats Team
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as
# published by the Free Software Foundation, either version 3 of the
# License, or (at your option) any later version.
# 
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
# 
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

set -e

# 1. Criando o usuário e o banco de dados (no banco principal/default)
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
	CREATE USER n8n_user WITH ENCRYPTED PASSWORD '$N8N_DB_PASSWORD';
	CREATE DATABASE n8n_db;
	GRANT ALL PRIVILEGES ON DATABASE n8n_db TO n8n_user;

	CREATE USER plane_user WITH ENCRYPTED PASSWORD '$PLANE_DB_PASSWORD';
	CREATE DATABASE plane_db;
	GRANT ALL PRIVILEGES ON DATABASE plane_db TO plane_user;

	CREATE USER grafana_reader WITH ENCRYPTED PASSWORD '$GRAFANA_READER_PASSWORD';
	GRANT CONNECT ON DATABASE eleverats_db TO grafana_reader;

	CREATE USER metabase_user WITH ENCRYPTED PASSWORD '$METABASE_DB_PASSWORD';
	CREATE DATABASE metabase_db;
	GRANT ALL PRIVILEGES ON DATABASE metabase_db TO metabase_user;
EOSQL

# 2. O Remédio para o Postgres 15+:
# Conectamos especificamente no n8n_db para dar o controle do schema public
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "n8n_db" <<-EOSQL
	GRANT ALL ON SCHEMA public TO n8n_user;
	ALTER SCHEMA public OWNER TO n8n_user;
EOSQL

# The Postgres 15+ remedy applied to Plane DB
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "plane_db" <<-EOSQL
	GRANT ALL ON SCHEMA public TO plane_user;
	ALTER SCHEMA public OWNER TO plane_user;
EOSQL

# The Postgres 15+ remedy applied to Metabase DB
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "metabase_db" <<-EOSQL
	GRANT ALL ON SCHEMA public TO metabase_user;
	ALTER SCHEMA public OWNER TO metabase_user;
EOSQL
# Permissionamento para o Grafana (Read-only)
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "eleverats_db" <<-EOSQL
	GRANT USAGE ON SCHEMA public TO grafana_reader;
	GRANT SELECT ON ALL TABLES IN SCHEMA public TO grafana_reader;
	ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO grafana_reader;
EOSQL
