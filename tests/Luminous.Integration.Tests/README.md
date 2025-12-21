# Luminous Integration Tests

This project contains integration tests that test the API endpoints with a real database connection.

## Prerequisites

- Azure Cosmos DB Emulator running locally
- Docker (optional, for running Redis and Azurite)

## Running the Tests

```bash
# Start local services
docker-compose up -d

# Run integration tests
dotnet test tests/Luminous.Integration.Tests
```

## Test Categories

- **API Tests**: Full API endpoint tests with authentication
- **Repository Tests**: Data access layer tests with Cosmos DB Emulator
- **End-to-End Tests**: Complete user journey tests

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `COSMOS_ENDPOINT` | Cosmos DB endpoint | `https://localhost:8081` |
| `COSMOS_KEY` | Cosmos DB key | Emulator key |
| `REDIS_CONNECTION` | Redis connection string | `localhost:6379` |
