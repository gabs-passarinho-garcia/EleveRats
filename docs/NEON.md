# 🐘 Running EleveRats on Neon Postgres

EleveRats leverages **Neon Serverless Postgres** to provide high-performance database capabilities. As an **AGPLv3** project, we focus on cloud-native scalability and transparency.

## ⚙️ Configuration (.NET 10)

### 1. Connection Pooling

Use the pooled connection string from Neon in your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-pooler-name.aws.neon.tech;Database=neondb;Username=user;Password=pass;SslMode=VerifyFull;"
  }
}
```

Or in .env file:

```bash
DATABASE_URL="postgresql://neondb_owner:[DB_PASSWORD]@[NEON_HOST]/eleverats_db?sslmode=require&channel_binding=require"
```

### 2. Running Migrations

```bash
just migration-add <MIGRATION_NAME>
just db-update
```

## ⚡ Why we chose Neon

- **Autoscaling:** Crucial for church events with hundreds of simultaneous check-ins.
- **Scale-to-Zero:** Keeps the project sustainable for small communities.
- **Branching:** Used in our CI/CD pipeline to test migrations before production.
