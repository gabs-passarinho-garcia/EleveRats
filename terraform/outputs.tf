output "public_ip" {
  description = "O IP público de acesso da sua Nave-Mãe na Oracle"
  value       = oci_core_instance.nave_mae.public_ip
}

output "ssh_ready_command" {
  description = "O comando mastigado para você colar no terminal e logar"
  value       = "ssh -i ~/.ssh/id_oracle_nave_mae ubuntu@${oci_core_instance.nave_mae.public_ip}"
}
