# Frontend - Dapr Operations Console (Angular 21)

This Angular app is a UI companion for the Dapr backend in the repository root.

## What It Demonstrates

- Calling backend service metadata and health endpoints
- Creating orders (`POST /orders`) to trigger state save + pub/sub publish
- Reading orders (`GET /orders/{id}`) from Dapr state store
- Manual publish (`POST /publish/orders`) to Dapr topic
- Service invocation (`POST /invoke/self`) through Dapr app ID

## Prerequisites

- Node.js 22.12.0 or newer
- Backend running with Dapr sidecar on `http://localhost:8080`

## Run

```powershell
npm install
npm start
```

Open `http://localhost:4200`.

## Build

```powershell
npm run build
```

## Main Files

- `src/app/app.ts`: UI logic and API actions
- `src/app/dapr-api.service.ts`: typed HTTP integration layer
- `src/app/models.ts`: backend payload types
- `src/app/app.html`: operational dashboard UI
- `src/app/app.scss`: styling
