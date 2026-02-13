# CartService

Cart microservice with state and subscription behavior.

## Dapr App ID

`cart-service`

## Endpoints

- `GET /`
- `GET /cart/{customerId}`
- `POST /cart/{customerId}/items`
- `DELETE /cart/{customerId}`
- `POST /checkout-events` (Dapr topic route)

## Dapr Usage

- Reads/writes cart state in `statestore`.
- Subscribes to `checkout.completed.v1` on `pubsub` and clears cart after checkout.

## Local Run

```powershell
cd microservices/CartService
dapr run --app-id cart-service --app-port 8082 --dapr-http-port 3502 --resources-path ../../DaprAcaStarter/components -- dotnet run --urls http://localhost:8082
```

## Docker

```powershell
docker build -t cart-service:local .
```
