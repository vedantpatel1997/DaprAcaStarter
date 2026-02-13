# ProductsService

Catalog microservice.

## Dapr App ID

`products-service`

## Endpoints

- `GET /`
- `GET /products`
- `GET /products/{id}`

## Local Run

```powershell
cd microservices/ProductsService
dapr run --app-id products-service --app-port 8081 --dapr-http-port 3501 --resources-path ../../DaprAcaStarter/components -- dotnet run --urls http://localhost:8081
```

## Docker

```powershell
docker build -t products-service:local .
```
