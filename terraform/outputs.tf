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

output "comando_recuperar_senha" {
  description = "Execute este comando na sua máquina com OCI CLI configurado para ver a senha do banco gerada (ou acesse a interface da OCI no painel do Vault):"
  value       = "oci vault secret-bundle get --secret-id ${oci_vault_secret.db_password_secret.id} --query 'data.\"secret-bundle-content\".content' --raw-output | base64 --decode"
}
