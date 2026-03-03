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

# 5. Security List Minimalista (Apenas SSH porta 22)
resource "oci_core_security_list" "sl_ssh_only" {
  compartment_id = var.compartment_ocid
  vcn_id         = oci_core_vcn.vcn_nave_mae.id
  display_name   = "sl-cloudflare-tunnel-safe"

  ingress_security_rules {
    protocol = "6" # TCP
    source   = "0.0.0.0/0"
    tcp_options {
      min = 22
      max = 22
    }
  }

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
  security_list_ids = [oci_core_security_list.sl_ssh_only.id]
}

# 7. A Instância Ampere (A "Nave-Mãe")
resource "oci_core_instance" "nave_mae" {
  availability_domain = var.availability_domain #
  compartment_id      = var.compartment_ocid
  display_name        = "Nave-Mae-Gabs"
  shape               = "VM.Standard.A1.Flex" #

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
    assign_public_ip = true
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
}

resource "oci_core_volume" "dados_nave_mae" {
  # Ensure the volume is created in the same Availability Domain as the instance
  availability_domain = var.availability_domain
  compartment_id      = var.compartment_ocid
  display_name        = "vol-dados-nave-mae"
  size_in_gbs         = 130
}

resource "oci_core_volume_attachment" "dados_nave_mae_attachment" {
  # Paravirtualized attachment simplifies OS-level volume discovery
  attachment_type = "paravirtualized"
  instance_id     = oci_core_instance.nave_mae.id
  volume_id       = oci_core_volume.dados_nave_mae.id
  display_name    = "att-dados-nave-mae"
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

