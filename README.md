# Dapr ACA Starter (.NET 10)

This project is a minimal ASP.NET Core service that demonstrates core Dapr building blocks and how the same app can run locally and in Azure Container Apps (ACA) with Dapr enabled.

## What This Project Is Doing

The service exposes HTTP endpoints for a simple `Order` workflow and uses Dapr for:

- State management (`statestore`) to persist orders
- Pub/Sub (`pubsub`) to publish and consume order events
- Service invocation to call an internal endpoint via Dapr app ID

It is a starter template for event-driven microservices patterns, not a full production system.

## How It Works Overall

1. Client calls `POST /orders`.
2. App creates an order ID and writes the order to Dapr state store (`statestore`).
3. App publishes the order to Dapr topic `orders.v1` on component `pubsub`.
4. Dapr routes that event back to the app subscription endpoint `POST /dapr/orders-subscription`.
5. You can fetch the stored order with `GET /orders/{id}`.
6. You can test Dapr service invocation with `POST /invoke/self`, which calls `internal/echo` through Dapr by app ID (`dapr-aca-starter`).

## Architecture (Local)

- App: ASP.NET Core minimal API on port `8080`
- Dapr sidecar: started by `dapr run`, HTTP API on `3500`
- Backend for Dapr components: local Redis (`localhost:6379`)

`components/statestore.yaml` and `components/pubsub.yaml` both point to local Redis.

## API Endpoints

- `GET /` service info
- `GET /healthz` health check
- `POST /orders` create order, save state, publish event
- `GET /orders/{id}` read order from state store
- `POST /publish/orders` manual event publish
- `POST /dapr/orders-subscription` event subscriber endpoint
- `POST /internal/echo` internal endpoint (invoked through Dapr)
- `POST /invoke/self` trigger Dapr service invocation call

## Use Cases

- Learn Dapr fundamentals in one service
- Validate local-to-cloud parity (same app model on ACA)
- Prototype event-driven order or workflow services
- Baseline for splitting into multiple services later (order, inventory, shipping, notifications)

## Prerequisites

- .NET SDK 10.0+
- Dapr CLI (initialized via `dapr init`)
- Docker Desktop
- Redis (local)

## Run Locally

From project root:

```powershell
# Start Redis
docker run --name dapr-redis -d -p 6379:6379 redis:7

# Run app + Dapr sidecar
dapr run --app-id dapr-aca-starter --app-port 8080 --dapr-http-port 3500 --resources-path ./components -- dotnet run --urls http://localhost:8080
```

OpenAPI document in development:
- `http://localhost:8080/openapi/v1.json`

## Quick Test

```powershell
# Health
curl http://localhost:8080/healthz

# Create order
curl -X POST http://localhost:8080/orders -H "Content-Type: application/json" -d '{"customerId":"cust-101","product":"Laptop Stand","quantity":2,"unitPrice":39.99}'

# Read order
curl http://localhost:8080/orders/<ORDER_ID>

# Manual publish
curl -X POST http://localhost:8080/publish/orders -H "Content-Type: application/json" -d '{"orderId":"<ORDER_ID>","status":"Shipped"}'

# Service invocation via Dapr
curl -X POST http://localhost:8080/invoke/self -H "Content-Type: application/json" -d '{"message":"hello via dapr invocation"}'
```

## Deploy To Azure Container Apps

`aca/containerapp.dapr.yaml` contains a sample ACA manifest with Dapr enabled.

1. Build and push image:

```powershell
docker build -t <ACR_LOGIN_SERVER>/dapr-aca-starter:latest .
docker push <ACR_LOGIN_SERVER>/dapr-aca-starter:latest
```

2. Update placeholders in `aca/containerapp.dapr.yaml`:
- `<SUBSCRIPTION_ID>`
- `<RESOURCE_GROUP>`
- `<ACA_ENV_NAME>`
- `<ACR_LOGIN_SERVER>`

3. Deploy with Azure CLI using the YAML.

4. For cloud components, replace local Redis-based Dapr components with managed services (for example Azure Service Bus, Azure Cache for Redis, Cosmos DB, or Blob Storage as appropriate).

## Important Files

- `Program.cs` application endpoints and Dapr integration
- `components/statestore.yaml` local state store component
- `components/pubsub.yaml` local pub/sub component
- `aca/containerapp.dapr.yaml` ACA deployment manifest with Dapr config
- `Dockerfile` container build for ACA
