# CheckoutService

Checkout microservice.

## Dapr App ID

`checkout-service`

## Endpoints

- `GET /`
- `POST /checkout/{customerId}`
- `GET /orders/{orderId}`

## Dapr Usage

- Invokes `cart-service` to read cart.
- Persists order in `statestore`.
- Publishes `checkout.completed.v1` on `pubsub`.

## Local Run

```powershell
cd microservices/CheckoutService
dapr run --app-id checkout-service --app-port 8083 --dapr-http-port 3503 --resources-path ../../DaprAcaStarter/components -- dotnet run --urls http://localhost:8083
```

## Docker

```powershell
docker build -t checkout-service:local .
```
