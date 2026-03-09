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
# GERAÇÃO E ARMAZENAMENTO DA SENHA DO BANCO
# ==========================================

# 1. Gera uma senha forte aleatoriamente (16 caracteres, sem especiais pra não bugar a URI de conexão)
resource "random_password" "db_password" {
  length  = 16
  special = false
}

resource "random_string" "db_user" {
  length  = 10
  special = false
  numeric = false
  upper   = false
}

resource "random_string" "minio_user" {
  length  = 10
  special = false
  numeric = false
  upper   = false
}

resource "random_password" "minio_password" {
  length  = 20
  special = false
}

resource "random_password" "redis_password" {
  length  = 32
  special = false
}

resource "random_password" "grafana_password" {
  length  = 20
  special = false
}

resource "random_id" "n8n_encryption_key" {
  byte_length = 32
}

resource "random_password" "n8n_db_password" {
  length  = 20
  special = false
}

resource "random_password" "plane_db_password" {
  length  = 20
  special = false
}

resource "random_password" "rabbitmq_password" {
  length  = 20
  special = false
}

resource "random_password" "grafana_reader_password" {
  length  = 20
  special = false
}

resource "random_password" "plane_secret_key" {
  length  = 48
  special = false
}

resource "random_password" "plane_live_server_secret_key" {
  length  = 32
  special = false
}

resource "random_password" "metabase_db_password" {
  length  = 20
  special = false
}

# 2. Cria o Vault (Cofre) Always Free
resource "oci_kms_vault" "eleverats_vault" {
  compartment_id = var.compartment_ocid
  display_name   = "vault-eleverats"
  vault_type     = "DEFAULT" # DEFAULT é o tipo suportado no Always Free

  lifecycle {
    prevent_destroy = true
  }
}

# 3. Cria a Chave Master no Vault (Master Encryption Key Protegida por HSM)
resource "oci_kms_key" "eleverats_master_key" {
  compartment_id      = var.compartment_ocid
  display_name        = "key-eleverats-master"
  management_endpoint = oci_kms_vault.eleverats_vault.management_endpoint

  key_shape {
    algorithm = "AES"
    length    = 32 # AES-256
  }

  protection_mode = "HSM" # Hardware Security Module (Always Free suporta 20 versões de chave)
}

# 4. Salva a Senha do Banco como um Secret no Vault (Always Free suporta 150 secrets)
resource "oci_vault_secret" "db_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "db-password"
  description    = "Senha gerada automaticamente para o banco de dados PostgreSQL do EleveRats"

  secret_content {
    content      = base64encode(random_password.db_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "db_user_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "db-user"
  description    = "Usuario do banco de dados PostgreSQL do EleveRats"

  secret_content {
    content      = base64encode(random_string.db_user.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "minio_user_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "minio-user"
  description    = "Usuario do Minio Storage"

  secret_content {
    content      = base64encode(random_string.minio_user.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "minio_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "minio-password"
  description    = "Senha do Minio Storage"

  secret_content {
    content      = base64encode(random_password.minio_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "redis_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "redis-password"
  description    = "Senha do Redis Cache in-memory"

  secret_content {
    content      = base64encode(random_password.redis_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "grafana_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "grafana-password"
  description    = "Senha de Administrador do Painel Grafana"

  secret_content {
    content      = base64encode(random_password.grafana_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "n8n_encryption_key_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "n8n-encryption-key"
  description    = "Chave de Encriptacao do N8N"

  secret_content {
    content      = base64encode(random_id.n8n_encryption_key.hex)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "cf_tunnel_token_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "cf-tunnel-token"
  description    = "Token do Cloudflare Tunnel"

  secret_content {
    content      = base64encode(var.cf_tunnel_token)
    content_type = "BASE64"
    stage        = "CURRENT"
  }
}

resource "oci_vault_secret" "foundry_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "foundry-password"
  description    = "Senha do FoundryVTT"

  secret_content {
    content      = base64encode(var.foundry_password)
    content_type = "BASE64"
    stage        = "CURRENT"
  }
}

resource "oci_vault_secret" "foundry_username_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "foundry-username"
  description    = "Usuario do FoundryVTT"

  secret_content {
    content      = base64encode(var.foundry_username)
    content_type = "BASE64"
    stage        = "CURRENT"
  }
}

resource "oci_vault_secret" "foundry_admin_key_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "foundry-admin-key"
  description    = "Chave de administrador do FoundryVTT"

  secret_content {
    content      = base64encode(var.foundry_admin_key)
    content_type = "BASE64"
    stage        = "CURRENT"
  }
}

resource "oci_vault_secret" "n8n_db_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "n8n-db-password"
  description    = "Senha do usuario n8n_user no PostgreSQL"

  secret_content {
    content      = base64encode(random_password.n8n_db_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "plane_db_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "plane-db-password"
  description    = "Senha do usuario plane_user no PostgreSQL"

  secret_content {
    content      = base64encode(random_password.plane_db_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "rabbitmq_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "rabbitmq-password"
  description    = "Senha do RabbitMQ para o stack Plane"

  secret_content {
    content      = base64encode(random_password.rabbitmq_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "grafana_reader_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "grafana-reader-password"
  description    = "Senha do usuario grafana_reader (ready-only) no PostgreSQL"

  secret_content {
    content      = base64encode(random_password.grafana_reader_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "plane_secret_key_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "plane-secret-key"
  description    = "SECRET_KEY para o Django do Plane"

  secret_content {
    content      = base64encode(random_password.plane_secret_key.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "plane_live_server_secret_key_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "plane-live-server-secret-key"
  description    = "LIVE_SERVER_SECRET_KEY para o Plane"

  secret_content {
    content      = base64encode(random_password.plane_live_server_secret_key.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

resource "oci_vault_secret" "metabase_db_password_secret" {
  compartment_id = var.compartment_ocid
  vault_id       = oci_kms_vault.eleverats_vault.id
  key_id         = oci_kms_key.eleverats_master_key.id
  secret_name    = "metabase-db-password"
  description    = "Senha do usuario metabase_user (internal state) no PostgreSQL"

  secret_content {
    content      = base64encode(random_password.metabase_db_password.result)
    content_type = "BASE64"
    stage        = "CURRENT"
  }

  lifecycle {
    ignore_changes = [secret_content]
  }
}

# ==========================================
# IAM: PERMISSÕES PARA A NAVE-MÃE LER O COFRE
# ==========================================

# 1. Grupo Dinâmico (Identidade da sua VM)
resource "oci_identity_dynamic_group" "nave_mae_dg" {
  # O compartment_id de um Dynamic Group DEVE ser o Tenancy OCID (Raiz da conta)
  compartment_id = var.tenancy_ocid
  name           = "dg-nave-mae"
  description    = "Grupo dinamico contendo a Nave Mae"

  # Regra: Qualquer instância no compartimento fará parte do grupo (Best Practice OCI)
  matching_rule = "ALL {instance.compartment.id = '${var.compartment_ocid}'}"
}

# 2. Política de Acesso (A chave da porta)
resource "oci_identity_policy" "nave_mae_vault_policy" {
  # A política pode ficar no seu compartimento normal
  compartment_id = var.compartment_ocid
  name           = "policy-nave-mae-secrets"
  description    = "Permite que a Nave Mae leia os segredos no compartimento"

  statements = [
    # Permite ler a familia de segredos e o conteudo do bundle
    "Allow dynamic-group dg-nave-mae to read secret-family in compartment id ${var.compartment_ocid}",
    "Allow dynamic-group dg-nave-mae to read secret-bundles in compartment id ${var.compartment_ocid}",
    # Permite que a Vault API use a chave KMS por baixo dos panos para descriptografar o payload pro cliente
    "Allow dynamic-group dg-nave-mae to use keys in compartment id ${var.compartment_ocid}"
  ]
}
