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


variable "cloudflare_api_token" {
  description = "Token de API do Cloudflare"
  type        = string
  sensitive   = true
}

variable "cloudflare_account_id" {
  description = "Account ID do Cloudflare"
  type        = string
}

variable "r2_access_key" {
  description = "Access Key do R2"
  type        = string
  sensitive   = true
}

variable "r2_secret_key" {
  description = "Secret Key do R2"
  type        = string
  sensitive   = true
}

variable "neon_api_key" {
  description = "Token de API do Neon"
  type        = string
  sensitive   = true
}

variable "render_api_key" {
  description = "Token de API do Render"
  type        = string
  sensitive   = true
}

variable "render_owner_id" {
  description = "Owner ID do Render"
  type        = string
}

variable "upstash_api_key" {
  description = "Token de API do Upstash"
  type        = string
  sensitive   = true
}

variable "upstash_email" {
  description = "Email do Upstash"
  type        = string
}
