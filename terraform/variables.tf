variable "compartment_ocid" {
  description = "OCID do seu compartimento raiz na Oracle Cloud"
  type        = string
}

variable "availability_domain" {
  description = "O Domínio de Disponibilidade (AD) da sua região (ex: lqtP:US-ASHBURN-AD-1)"
  type        = string
}
