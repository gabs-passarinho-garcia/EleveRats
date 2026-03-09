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
    dotnet sonarscanner begin /k:"EleveRats_Backend" /o:"eleverats" /d:sonar.token="$SONAR_TOKEN" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.cs.opencover.reportsPaths="backend.tests/TestResults/**/coverage.opencover.xml"
    dotnet build
    dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
