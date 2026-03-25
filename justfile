# justfile for EleveRats 🐀

set dotenv-load := true

# Default to showing tasks
default:
    just --list

# Format code using dotnet format
format:
    dotnet format

# Build the entire solution
build:
    dotnet build

# Run all unit tests
test:
    dotnet test --no-build --verbosity normal

# Run tests with coverage using the cross-platform collector
test-coverage:
    dotnet test --collect:"XPlat Code Coverage;Format=opencover"

# Run Sonar analysis (requires SONAR_TOKEN env var, dotnet-sonarscanner and bun)
sonar:
    cd frontend && bun install && bun run test:cov
    # Fix paths in lcov.info to be relative to the solution root
    sed -i 's|SF:src/|SF:frontend/src/|g' frontend/coverage/lcov.info
    dotnet test --collect:"XPlat Code Coverage;Format=opencover" --results-directory backend.tests/TestResults
    dotnet sonarscanner begin /k:"EleveRats" \
        /d:sonar.token="$SONAR_TOKEN" \
        /d:sonar.host.url="${SONAR_HOST_URL:-https://sonarcloud.io}" \
        /d:sonar.cs.opencover.reportsPaths="backend.tests/TestResults/**/coverage.opencover.xml" \
        /d:sonar.javascript.lcov.reportPaths="frontend/coverage/lcov.info" \
        /d:sonar.exclusions="**/bin/**,**/obj/**,frontend/node_modules/**,backend/Migrations/**,backend/Services/AntiIdlenessService.cs"
    dotnet build
    dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"

# Local environment (Core only)
up-local:
    docker compose -f docker-compose.local.yml up -d

# Stop local environment
down-local:
    docker compose -f docker-compose.local.yml down

# Show logs for local environment
logs-local:
    docker compose -f docker-compose.local.yml logs -f

# --- Database & Migrations (EF Core) ---

# Create a new migration: just migration-add InitialCreate
migration-add name:
    dotnet ef migrations add {{ name }} --project backend

# Remove the last migration
migration-remove:
    dotnet ef migrations remove --project backend

# Apply pending migrations to the database
db-update:
    dotnet ef database update --project backend

# List all migrations
migration-list:
    dotnet ef migrations list --project backend
