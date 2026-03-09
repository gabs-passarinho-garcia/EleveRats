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

# Configuração do Provedor OCI
terraform {
  required_providers {
    oci = {
      source  = "oracle/oci"
      version = ">= 8.3.0"
    }
  }
  backend "s3" {
    bucket   = "eleverats-tf-state"
    key      = "prod/terraform.tfstate"
    region   = "us-ashburn-1"
    endpoint = "https://idyw9ukvm7n9.compat.objectstorage.us-ashburn-1.oraclecloud.com"

    # A MÁGICA DO ISOLAMENTO ACONTECE AQUI 👇
    profile = "oracle_eleverats"

    # Flags obrigatórias para APIs S3-compatíveis
    skip_region_validation      = true
    skip_credentials_validation = true
    skip_requesting_account_id  = true
    use_path_style              = true
  }
}

# 1. Busca automática da imagem Ubuntu 24.04 Minimal ARM
data "oci_core_images" "ubuntu_minimal_arm" {
  compartment_id           = var.compartment_ocid
  operating_system         = "Canonical Ubuntu"
  operating_system_version = "24.04 Minimal aarch64"
  shape                    = "VM.Standard.A1.Flex" #

  filter {
    name   = "display_name"
    values = ["^.*-aarch64-.*$"] # Garante que pegamos a versão 64-bit ARM
    regex  = true
  }
}

# 2. Rede Virtual (VCN)
resource "oci_core_vcn" "vcn_nave_mae" {
  compartment_id = var.compartment_ocid
  cidr_block     = "10.0.0.0/16"
  display_name   = "vcn-nave-mae"
}

# 3. Internet Gateway (Para a VM falar com o mundo/Cloudflare)
resource "oci_core_internet_gateway" "ig" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.vcn_nave_mae.id
  display_name   = "ig-nave-mae"
}

# 4. Tabela de Roteamento
resource "oci_core_route_table" "rt" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.vcn_nave_mae.id
  display_name   = "rt-nave-mae"
  route_rules {
    destination       = "0.0.0.0/0"
    destination_type  = "CIDR_BLOCK"
    network_entity_id = oci_core_internet_gateway.ig.id
  }
}

# 5. Security List: Zero inbound público — acesso via Tailscale only
resource "oci_core_security_list" "sl_tailscale_only" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.vcn_nave_mae.id
  display_name   = "sl-tailscale-only"

  # No ingress rules — all inbound traffic is blocked at the OCI level.
  # Access to the instance is exclusively through the Tailscale mesh network.

  egress_security_rules {
    protocol    = "all"
    destination = "0.0.0.0/0"
  }
}

# 6. Subrede
resource "oci_core_subnet" "subnet" {
  compartment_id    = var.compartment_ocid
  vcn_id            = oci_core_vcn.vcn_nave_mae.id
  cidr_block        = "10.0.1.0/24"
  display_name      = "subnet-nave-mae"
  route_table_id    = oci_core_route_table.rt.id
  security_list_ids = [oci_core_security_list.sl_tailscale_only.id]
}

# 7. A Instância Ampere (A "Nave-Mãe")
resource "oci_core_instance" "nave_mae" {
  availability_domain = var.availability_domain #
  compartment_id      = var.compartment_ocid
  display_name        = "Nave-Mae-Gabs-2"
  shape               = "VM.Standard.A1.Flex" #

  preserve_boot_volume = true

  shape_config {
    ocpus         = 4  # Limite Always Free
    memory_in_gbs = 24 # Limite Always Free
  }

  source_details {
    source_type = "image"
    source_id   = data.oci_core_images.ubuntu_minimal_arm.images[0].id
  }

  create_vnic_details {
    subnet_id        = oci_core_subnet.subnet.id
    assign_public_ip = false # Public access disabled; Tailscale handles all inbound
  }

  metadata = {
    # Aqui vai a sua chave Ed25519 gerada localmente
    ssh_authorized_keys = file(pathexpand("../eleve_rats_ssh.pub"))

    # Script Cloud-Init (Instala Docker e trava portas internas)
    # Renderizado usando templatefile para passar a referência do Secret na OCI.
    user_data = base64encode(templatefile("init.sh", {
      db_password_secret_id        = oci_vault_secret.db_password_secret.id
      db_user_secret_id            = oci_vault_secret.db_user_secret.id
      minio_user_secret_id         = oci_vault_secret.minio_user_secret.id
      minio_password_secret_id     = oci_vault_secret.minio_password_secret.id
      redis_password_secret_id     = oci_vault_secret.redis_password_secret.id
      grafana_password_secret_id   = oci_vault_secret.grafana_password_secret.id
      n8n_encryption_key_secret_id = oci_vault_secret.n8n_encryption_key_secret.id
      cf_tunnel_token_secret_id    = oci_vault_secret.cf_tunnel_token_secret.id
      foundry_password_secret_id   = oci_vault_secret.foundry_password_secret.id
      foundry_username_secret_id   = oci_vault_secret.foundry_username_secret.id
      foundry_admin_key_secret_id  = oci_vault_secret.foundry_admin_key_secret.id
      n8n_db_password_secret_id        = oci_vault_secret.n8n_db_password_secret.id
      plane_db_password_secret_id      = oci_vault_secret.plane_db_password_secret.id
      rabbitmq_password_secret_id      = oci_vault_secret.rabbitmq_password_secret.id
      grafana_reader_password_secret_id = oci_vault_secret.grafana_reader_password_secret.id
      plane_secret_key_secret_id       = oci_vault_secret.plane_secret_key_secret.id
      plane_live_server_secret_key_secret_id = oci_vault_secret.plane_live_server_secret_key_secret.id
      metabase_db_password_secret_id = oci_vault_secret.metabase_db_password_secret.id
    }))
  }

  instance_options {
    are_legacy_imds_endpoints_disabled = true # Segurança IMDSv2
  }

  agent_config {
    plugins_config {
      name          = "Compute Instance Monitoring"
      desired_state = "ENABLED" #
    }
  }

  lifecycle {
    prevent_destroy = true
    # Ignore changes to metadata (user_data, ssh keys) and source_details (image updates).
    # The instance is long-lived; we don't want Tofu to rebuild it every time Oracle
    # releases a new Ubuntu image.
    ignore_changes = [metadata, source_details]
  }
}

resource "oci_core_volume" "dados_nave_mae" {
  # Ensure the volume is created in the same Availability Domain as the instance
  availability_domain = var.availability_domain
  compartment_id      = var.compartment_ocid
  display_name        = "vol-dados-nave-mae"
  size_in_gbs         = 130

  lifecycle {
    prevent_destroy = true
  }
}

resource "oci_core_volume_attachment" "dados_nave_mae_attachment" {
  # Paravirtualized attachment simplifies OS-level volume discovery
  attachment_type = "paravirtualized"
  instance_id     = oci_core_instance.nave_mae.id
  volume_id       = oci_core_volume.dados_nave_mae.id
  display_name    = "att-dados-nave-mae"
}

# 8. IP Público Reservado (Persistente) — nunca muda, nunca morre
# Attached to the instance's primary VNIC after provisioning.
resource "oci_core_public_ip" "nave_mae_reserved_ip" {
  compartment_id = var.compartment_ocid
  lifetime       = "RESERVED"
  display_name   = "reserved-ip-nave-mae"

  # Binds the reserved IP to the instance's primary private IP.
  # The OCID of the private IP is resolved via oci_core_private_ips.
  private_ip_id = data.oci_core_private_ips.nave_mae_private_ips.private_ips[0].id

  lifecycle {
    prevent_destroy = true
  }
}

# Data source to resolve the primary VNIC of the instance
data "oci_core_vnic_attachments" "nave_mae_vnic_attachments" {
  compartment_id      = var.compartment_ocid
  availability_domain = var.availability_domain
  instance_id         = oci_core_instance.nave_mae.id
}

data "oci_core_vnic" "nave_mae_vnic" {
  vnic_id = data.oci_core_vnic_attachments.nave_mae_vnic_attachments.vnic_attachments[0].vnic_id
}

# Resolves the OCID of the primary private IP attached to the VNIC.
# Required to bind a Reserved Public IP to the instance.
data "oci_core_private_ips" "nave_mae_private_ips" {
  vnic_id = data.oci_core_vnic_attachments.nave_mae_vnic_attachments.vnic_attachments[0].vnic_id
}

# 1. Pega o seu "Namespace" único da Oracle (obrigatório pro Object Storage)
data "oci_objectstorage_namespace" "user_namespace" {
  compartment_id = var.compartment_ocid
}

# 2. Cria o Bucket S3-Compatible Always Free
resource "oci_objectstorage_bucket" "tf_state" {
  compartment_id = var.compartment_ocid
  namespace      = data.oci_objectstorage_namespace.user_namespace.namespace
  name           = "eleverats-tf-state"

  # Segurança máxima, ninguém de fora lê o seu estado
  access_type = "NoPublicAccess"

  # Atitude de Sênior: Habilitar versionamento. 
  # Se der merda no estado, você consegue voltar no tempo!
  versioning = "Enabled"
}

