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

# 2. Cria o Vault (Cofre) Always Free
resource "oci_kms_vault" "eleverats_vault" {
  compartment_id = var.compartment_ocid
  display_name   = "vault-eleverats"
  vault_type     = "DEFAULT" # DEFAULT é o tipo suportado no Always Free
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

  # Regra: Qualquer instância que tiver o exato ID da sua Nave-Mãe fará parte do grupo
  matching_rule = "Any {instance.id = '${oci_core_instance.nave_mae.id}'}"
}

# 2. Política de Acesso (A chave da porta)
resource "oci_identity_policy" "nave_mae_vault_policy" {
  # A política pode ficar no seu compartimento normal
  compartment_id = var.compartment_ocid
  name           = "policy-nave-mae-secrets"
  description    = "Permite que a Nave Mae leia os segredos no compartimento"

  statements = [
    # Permite ler apenas o conteúdo do segredo (secret-bundles)
    "Allow dynamic-group dg-nave-mae to read secret-bundles in compartment id ${var.compartment_ocid}"
  ]
}
