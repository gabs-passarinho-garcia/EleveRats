# Configuração do Provedor OCI
terraform {
  required_providers {
    oci = {
      source  = "oracle/oci"
      version = ">= 4.0.0"
    }
  }
}

# 1. Busca automática da imagem Ubuntu 24.04 Minimal ARM
data "oci_core_images" "ubuntu_minimal_arm" {
  compartment_id           = var.compartment_ocid
  operating_system         = "Canonical Ubuntu"
  operating_system_version = "24.04 Minimal"
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
    ssh_authorized_keys = file("~/.ssh/id_oracle_nave_mae.pub")
    
    # Script Cloud-Init (Instala Docker e trava portas internas)
    user_data = base64encode(file("init.sh")) #
  }

  instance_options {
    are_legacy_imds_endpoints_allowed = false # Segurança IMDSv2
  }

  agent_config {
    plugins_config {
      name          = "Compute Instance Monitoring"
      desired_state = "ENABLED" #
    }
  }
}

output "public_ip" {
  value = oci_core_instance.nave_mae.public_ip
}
