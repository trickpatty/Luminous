# OpenAPI Specification

This directory contains the OpenAPI specification for the Luminous API.

## Files

- `v1.json` - The OpenAPI v3.1 specification (auto-generated)

## Generation

The OpenAPI specification is automatically generated from the .NET API when:
1. The API is built and run
2. The GitHub Actions workflow `openapi-clients.yml` runs

## Usage

### Generate TypeScript Types

```bash
npm install -g openapi-typescript
openapi-typescript v1.json -o ../clients/shared/src/lib/api-client/api-types.ts
```

### Generate Swift Client

```bash
swift-openapi-generator generate v1.json --output-directory ../clients/ios/Generated
```

### Generate Kotlin Client

```bash
openapi-generator-cli generate -i v1.json -g kotlin -o ../clients/android/generated
```

## Manual Export

To manually export the OpenAPI specification:

1. Run the API locally:
   ```bash
   cd src/Luminous.Api
   dotnet run
   ```

2. Download the spec:
   ```bash
   curl http://localhost:5000/openapi/v1.json -o openapi/v1.json
   ```

Or visit the Swagger UI at `http://localhost:5000/swagger` to view and download the spec.
