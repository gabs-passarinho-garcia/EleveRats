# 🏗️ Infrastructure as Code (IaC)

EleveRats infrastructure is fully automated using **OpenTofu** (the open-source fork of Terraform). This ensures that any community can replicate our stack in minutes.

## Stack

- **Database:** Neon Postgres (Serverless)
- **Cache:** Upstash Redis
- **Hosting:** Render (Web Services)
- **State Storage:** Cloudflare R2

## Deployment

1. **Initialize:** `tofu init`
2. **Plan:** `tofu plan -var-file="terraform.tfvars"`
3. **Apply:** `tofu apply -var-file="terraform.tfvars"`

The state is securely stored in a private Cloudflare R2 bucket, managed via GitHub Actions.
