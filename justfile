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
    dotnet test --collect:"XPlat Code Coverage"

# Run Sonar analysis (requires SONAR_TOKEN env var and dotnet-sonarscanner tool)
sonar:
    dotnet sonarscanner begin /k:"EleveRats" /d:sonar.token="$SONAR_TOKEN" /d:sonar.host.url="${SONAR_HOST_URL:-https://sonarcloud.io}" /d:sonar.cs.opencover.reportsPaths="backend.tests/TestResults/**/coverage.opencover.xml"
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
