terraform {
  required_providers {
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 5.0"
    }
    neon = {
      source  = "kislerdm/neon"
      version = "~> 0.13"
    }
    render = {
      source  = "render-oss/render"
      version = "~> 1.8"
    }
    upstash = {
      source  = "upstash/upstash"
      version = "~> 2.0"
    }
  }
}

provider "cloudflare" {
  api_token = var.cloudflare_api_token
}

provider "neon" {
  api_key = var.neon_api_key
}

provider "render" {
  api_key  = var.render_api_key
  owner_id = var.render_owner_id
}

provider "upstash" {
  api_key = var.upstash_api_key
  email   = var.upstash_email
}
