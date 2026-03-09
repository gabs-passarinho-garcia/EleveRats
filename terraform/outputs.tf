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

# ==========================================
# OUTPUTS ÚTEIS
# ==========================================

output "public_ip" {
  description = "O IP público de acesso da sua Nave-Mãe na Oracle"
  value       = oci_core_instance.nave_mae.public_ip
}

output "ssh_ready_command" {
  description = "O comando mastigado para você colar no terminal e logar"
  value       = "ssh -i ~/.ssh/id_oracle_nave_mae ubuntu@${oci_core_instance.nave_mae.public_ip}"
}

# 3. Manda o Tofu te devolver a URL exata do seu endpoint S3-Compatible
output "oracle_s3_endpoint" {
  description = "Copie isso e cole no 'endpoint' do seu bloco backend s3"
  value       = "https://${data.oci_objectstorage_namespace.user_namespace.namespace}.compat.objectstorage.us-ashburn-1.oraclecloud.com"
}

output "db_password_secret_id" {
  description = "OCID do Segredo da Senha do Banco de Dados no Vault"
  value       = oci_vault_secret.db_password_secret.id
}

output "db_user_secret_id" {
  description = "OCID do Segredo do Usuario do Banco de Dados no Vault"
  value       = oci_vault_secret.db_user_secret.id
}

output "minio_user_secret_id" {
  description = "OCID do Segredo do Usuario do Minio no Vault"
  value       = oci_vault_secret.minio_user_secret.id
}

output "minio_password_secret_id" {
  description = "OCID do Segredo da Senha do Minio no Vault"
  value       = oci_vault_secret.minio_password_secret.id
}

output "redis_password_secret_id" {
  description = "OCID do Segredo da Senha do Redis no Vault"
  value       = oci_vault_secret.redis_password_secret.id
}

output "grafana_password_secret_id" {
  description = "OCID do Segredo da Senha do Grafana no Vault"
  value       = oci_vault_secret.grafana_password_secret.id
}

output "n8n_encryption_key_secret_id" {
  description = "OCID do Segredo da Chave de Encriptacao do n8n no Vault"
  value       = oci_vault_secret.n8n_encryption_key_secret.id
}

output "cf_tunnel_token_secret_id" {
  description = "OCID do Segredo do Token do Cloudflare Tunnel no Vault"
  value       = oci_vault_secret.cf_tunnel_token_secret.id
}

output "foundry_password_secret_id" {
  description = "OCID do Segredo da Senha do FoundryVTT no Vault"
  value       = oci_vault_secret.foundry_password_secret.id
}

output "foundry_username_secret_id" {
  description = "OCID do Segredo do Usuario do FoundryVTT no Vault"
  value       = oci_vault_secret.foundry_username_secret.id
}

output "foundry_admin_key_secret_id" {
  description = "OCID do Segredo da Chave de Administrador do FoundryVTT no Vault"
  value       = oci_vault_secret.foundry_admin_key_secret.id
}

output "n8n_db_password_secret_id" {
  description = "OCID do Segredo da Senha do Banco de Dados do n8n no Vault"
  value       = oci_vault_secret.n8n_db_password_secret.id
}

output "plane_db_password_secret_id" {
  description = "OCID do Segredo da Senha do Banco de Dados do Plane no Vault"
  value       = oci_vault_secret.plane_db_password_secret.id
}

output "rabbitmq_password_secret_id" {
  description = "OCID do Segredo da Senha do RabbitMQ no Vault"
  value       = oci_vault_secret.rabbitmq_password_secret.id
}

output "grafana_reader_password_secret_id" {
  description = "OCID do Segredo da Senha do Grafana Reader no Vault"
  value       = oci_vault_secret.grafana_reader_password_secret.id
}

output "plane_secret_key_secret_id" {
  description = "OCID do Segredo da SECRET_KEY do Plane no Vault"
  value       = oci_vault_secret.plane_secret_key_secret.id
}

output "plane_live_server_secret_key_secret_id" {
  description = "OCID do Segredo da LIVE_SERVER_SECRET_KEY do Plane no Vault"
  value       = oci_vault_secret.plane_live_server_secret_key_secret.id
}

output "metabase_db_password_secret_id" {
  description = "OCID do Segredo da Senha do Banco de Dados do Metabase no Vault"
  value       = oci_vault_secret.metabase_db_password_secret.id
}

output "comando_recuperar_senha" {
  description = "Execute este comando na sua máquina com OCI CLI configurado para ver a senha do banco gerada (ou acesse a interface da OCI no painel do Vault):"
  value       = "oci vault secret-bundle get --secret-id ${oci_vault_secret.db_password_secret.id} --query 'data.\"secret-bundle-content\".content' --raw-output | base64 --decode"
}

output "comando_recuperar_senha_metabase" {
  description = "Execute este comando para ver a senha do Metabase:"
  value       = "oci vault secret-bundle get --secret-id ${oci_vault_secret.metabase_db_password_secret.id} --query 'data.\"secret-bundle-content\".content' --raw-output | base64 --decode"
}
