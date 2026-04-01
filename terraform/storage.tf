resource "cloudflare_r2_bucket" "assets" {
  account_id = var.cloudflare_account_id
  name       = "eleverats-assets"
  location   = "ENAM" # Choose an appropriate location
}

resource "cloudflare_r2_bucket" "state" {
  account_id = var.cloudflare_account_id
  name       = "eleverats-state"
  location   = "ENAM"
}
