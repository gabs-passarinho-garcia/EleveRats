with open(".github/workflows/infra-deploy.yml", "r") as f:
    content = f.read()

content = content.replace(
    "if: github.event_name == 'pull_request' && env.TF_VAR_cloudflare_account_id != ''",
    "if: github.event_name == 'pull_request' && env.TF_VAR_cloudflare_account_id != ''"
)
# Wait, env context is not available at the job level `if`. We should use secrets directly.
content = content.replace(
    "if: github.event_name == 'pull_request' && env.TF_VAR_cloudflare_account_id != ''",
    "if: github.event_name == 'pull_request' && secrets.CLOUDFLARE_ACCOUNT_ID != ''"
)
with open(".github/workflows/infra-deploy.yml", "w") as f:
    f.write(content)
