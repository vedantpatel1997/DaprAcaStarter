# Storefront API (`DaprAcaStarter`)

This project is the backend-for-frontend (BFF) for the Angular app.

## Responsibility

- Expose public API for UI.
- Route requests to downstream microservices using Dapr service invocation.
- Return aggregated/normalized responses.

## Key Endpoints

- `GET /` service metadata
- `GET /healthz`
- `GET /api/products`
- `GET /api/cart/{customerId}`
- `POST /api/cart/{customerId}/items`
- `POST /api/checkout/{customerId}`
- `GET /api/orders/{orderId}`

## Internal Design

- `Controllers/StorefrontController.cs`: API endpoints.
- `Services/StorefrontService.cs`: Dapr invocation logic.
- `Services/AppInfoService.cs`: metadata/health payload.
- `Configuration/DaprOptions.cs`: app IDs/component names.

## Dapr Settings

Configured in `appsettings.json` and `appsettings.Development.json`:

- `AppId`: `storefront-api`
- `StateStoreName`: `statestore`
- `PubSubName`: `pubsub`
- `OrdersTopic`: `checkout.completed.v1`
- Downstream app IDs:
  - `products-service`
  - `cart-service`
  - `checkout-service`

## Local Run

```powershell
cd DaprAcaStarter
dapr run --app-id storefront-api --app-port 8080 --dapr-http-port 3500 --resources-path ./components -- dotnet run --urls http://localhost:8080
```

## HTTP Test File

Use `DaprAcaStarter.http` to quickly test all storefront routes.

## Docker

```powershell
docker build -t storefront-api:local .
```

## ACA Deployment

Manifest: `aca/containerapp.dapr.yaml`.

Set image before deployment:

```powershell
# in aca/containerapp.dapr.yaml
# image: <ACR_LOGIN_SERVER>/storefront-api:<TAG>
```

Deploy or update:

```powershell
az containerapp create -g <RG> --yaml aca/containerapp.dapr.yaml
# or
az containerapp update -g <RG> --yaml aca/containerapp.dapr.yaml
```
