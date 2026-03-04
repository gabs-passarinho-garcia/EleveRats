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

output "comando_recuperar_senha" {
  description = "Execute este comando na sua máquina com OCI CLI configurado para ver a senha do banco gerada (ou acesse a interface da OCI no painel do Vault):"
  value       = "oci vault secret-bundle get --secret-id ${oci_vault_secret.db_password_secret.id} --query 'data.\"secret-bundle-content\".content' --raw-output | base64 --decode"
}
