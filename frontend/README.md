# Frontend (Angular 21)

This frontend is a workflow UI for the Dapr storefront architecture.

## What It Does

- Calls `storefront-api` endpoints.
- Displays products, cart, order details, metadata, health, and activity logs.
- Demonstrates user flow:
  - load products
  - add item to cart
  - checkout
  - verify cart is cleared by pub/sub subscriber

## Runtime API Base URL

Default API URL is configured in `src/app/dapr-api.service.ts`.

- Local: `http://localhost:8080`
- Cloud example: `https://storefront-api.blackocean-54455b91.westus2.azurecontainerapps.io`

The UI also allows changing API URL at runtime.

## Commands

```powershell
npm install
npm start
npm run build
```

## Main Files

- `src/app/app.ts`: page behavior and API actions
- `src/app/dapr-api.service.ts`: typed HTTP client
- `src/app/models.ts`: frontend models
- `src/app/app.html`: dashboard template
- `src/app/app.scss`: styles

## Docker

```powershell
docker build -t storefront-frontend:local .
```

Container serves static files via Nginx (`nginx.conf`).

## ACA

Deploy image as a separate container app with external ingress on port `80`.
