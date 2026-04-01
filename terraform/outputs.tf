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

# ==========================================
# OUTPUTS ÚTEIS - A NOVA NAVE-MÃE
# ==========================================

# --- Cloudflare R2 ---
output "r2_assets_bucket_name" {
  description = "Nome do bucket R2 para assets"
  value       = cloudflare_r2_bucket.assets.name
}

output "r2_state_bucket_id" {
  description = "ID do bucket R2 para o terraform state"
  value       = cloudflare_r2_bucket.state.id
}

# --- Neon (Postgres) ---
output "neon_project_id" {
  description = "ID do Projeto no Neon"
  value       = neon_project.main.id
}

# --- Render (Hosting) ---
output "render_api_service_id" {
  description = "ID do Serviço no Render"
  value       = render_service.api.id
}

# --- Upstash (Redis) ---
output "upstash_redis_endpoint" {
  description = "Endpoint do cache Redis"
  value       = upstash_redis_database.cache.endpoint
}

