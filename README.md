# Dapr ACA Starter (.NET 10 + Angular 21)

This repository demonstrates a local Dapr-based microservice workflow with:

- Backend API: `DaprAcaStarter` (ASP.NET Core + Dapr SDK)
- Frontend UI: `frontend` (Angular)
- Infrastructure dependency: Redis (state store + pub/sub broker)

The project is designed for local development with:

- Visual Studio for backend code/debugging
- VS Code for frontend development
- Docker Desktop for Redis
- Dapr sidecar running locally

## Architecture Overview

### Backend (`DaprAcaStarter`)

- `Controllers/`: HTTP endpoints and Dapr subscription handlers
- `Services/`: business logic for state, pub/sub, and service invocation
- `Models/`: request/response contracts and entities
- `Configuration/`: Dapr option defaults and binding models
- `Extensions/`: DI registration and HTTP pipeline setup
- `components/`: Dapr component manifests (`statestore`, `pubsub`)

### Frontend (`frontend`)

- Angular UI for invoking backend APIs
- Default backend URL in UI service: `http://localhost:8080`
- Can be changed from the UI/API base URL input at runtime

## What This Project Demonstrates

- Dapr state management using Redis component `statestore`
- Dapr pub/sub using Redis component `pubsub` and topic `orders.v1`
- Dapr service invocation using app ID `dapr-aca-starter`
- Dapr subscription handling via `[Topic(...)]` endpoint

## Local API Endpoints

- `GET /`
- `GET /healthz`
- `POST /orders`
- `GET /orders/{id}`
- `POST /publish/orders`
- `POST /dapr/orders-subscription`
- `POST /internal/echo`
- `POST /invoke/self`

## Prerequisites

Install and verify:

- .NET SDK 10.x
- Node.js 22.12.0+
- npm 10.9.0+
- Docker Desktop (running)
- Dapr CLI + runtime (`dapr init` done)

Verification commands:

```powershell
dotnet --list-sdks
node -v
npm -v
docker --version
dapr --version
```

## First-Time Setup

### 1) Start Redis container

```powershell
docker run --name dapr-redis -d -p 6379:6379 redis:7
```

If container already exists:

```powershell
docker start dapr-redis
```

### 2) Install frontend dependencies

```powershell
cd frontend
npm install
```

## Run Modes

## Mode A: Full Terminal Run (quickest, no IDE dependency)

Use this when you want everything aligned to backend `8080` (frontend default).

### Terminal 1: Backend + Dapr sidecar

```powershell
cd DaprAcaStarter
dapr run --app-id dapr-aca-starter --app-port 8080 --dapr-http-port 3500 --resources-path ./components -- dotnet run --urls http://localhost:8080
```

### Terminal 2: Frontend

```powershell
cd frontend
npm start
```

Open `http://localhost:4200`.

## Mode B: Visual Studio (backend) + VS Code (frontend)

Use this when you want debugging in Visual Studio.

By default Visual Studio launch profile runs backend on `http://localhost:5002`.

### Terminal 1: Start Dapr sidecar only

```powershell
cd DaprAcaStarter
dapr run --app-id dapr-aca-starter --app-port 5002 --dapr-http-port 3500 --resources-path ./components
```

### Visual Studio: Start backend app

1. Open `Dapr ACA.sln`
2. Select launch profile `http`
3. Press `F5`

### VS Code / Terminal 2: Start frontend

```powershell
cd frontend
npm start
```

Open `http://localhost:4200`.

Important: set frontend API base URL to `http://localhost:5002` (UI default is `http://localhost:8080`).

## Dapr Flow (End-to-End)

This is the runtime flow when you create an order from frontend:

1. Frontend sends `POST /orders` to backend API.
2. `OrdersController` calls `OrderService.CreateOrderAsync`.
3. `OrderService` writes order state via Dapr SDK:
   - `SaveStateAsync("statestore", orderId, order)`
4. Dapr sidecar resolves `statestore` from `components/statestore.yaml`.
5. Dapr component writes data to Redis (`localhost:6379`).
6. `OrderService` publishes event via Dapr SDK:
   - `PublishEventAsync("pubsub", "orders.v1", order)`
7. Dapr sidecar resolves `pubsub` from `components/pubsub.yaml`.
8. Redis pub/sub carries the message.
9. Dapr discovers backend subscription via `/dapr/subscribe` (enabled by `MapSubscribeHandler`).
10. Dapr delivers topic message to:
    - `POST /dapr/orders-subscription`
11. `SubscriptionController` handles message and logs receipt.
12. API returns created order response to frontend.

### Service Invocation Flow

When frontend calls `POST /invoke/self`:

1. `InvocationController` calls `InvocationService.InvokeSelfAsync`.
2. Service uses Dapr SDK `InvokeMethodAsync` with app ID `dapr-aca-starter`.
3. Dapr sidecar resolves target app by app ID.
4. Sidecar invokes backend endpoint `POST /internal/echo`.
5. Echo response is returned back through Dapr to caller.

## Dapr Component Files

- `components/statestore.yaml`
  - `type: state.redis`
  - `name: statestore`
  - `redisHost: localhost:6379`

- `components/pubsub.yaml`
  - `type: pubsub.redis`
  - `name: pubsub`
  - `redisHost: localhost:6379`

## Health and Validation Checklist

After startup, verify in this order:

1. Backend metadata:
```powershell
Invoke-RestMethod http://localhost:8080/
```
(or `http://localhost:5002/` in Mode B)

2. Health:
```powershell
Invoke-RestMethod http://localhost:8080/healthz
```

3. Create order (example):
```powershell
$order = Invoke-RestMethod -Method Post -Uri http://localhost:8080/orders -ContentType "application/json" -Body '{"customerId":"C-100","product":"Keyboard","quantity":1,"unitPrice":99.0}'
$order
```

4. Read order:
```powershell
Invoke-RestMethod "http://localhost:8080/orders/$($order.id)"
```

## Troubleshooting

- `Error: connection refused localhost:6379`
  - Redis container is not running. Run `docker start dapr-redis`.

- Frontend cannot call backend (CORS/network error)
  - Check backend port:
    - Mode A: `8080`
    - Mode B: `5002`
  - Ensure frontend API base URL matches backend URL.

- Dapr pub/sub or state not working
  - Ensure `dapr run` used `--resources-path ./components` from `DaprAcaStarter` directory.
  - Confirm sidecar logs show components loaded.

- Port already in use
  - Stop old processes/containers or change ports in command.

## Notes for ACA Deployment

The project includes `aca/containerapp.dapr.yaml` as a starting point for Azure Container Apps Dapr configuration. Local development in this README is self-hosted Dapr mode.
