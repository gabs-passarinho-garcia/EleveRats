resource "cloudflare_r2_bucket" "assets" {
  account_id = var.cloudflare_account_id
  name       = "eleverats-assets"
  location   = "ENAM" # Choose an appropriate location
}

resource "cloudflare_r2_custom_domain" "assets_public_domain" {
  account_id  = var.cloudflare_account_id
  bucket_name = cloudflare_r2_bucket.assets.name
  domain      = var.r2_custom_domain
  zone_id     = var.cloudflare_zone_id
  enabled     = true
}

