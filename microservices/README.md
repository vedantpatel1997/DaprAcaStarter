# Microservices

This folder contains backend microservices used by `storefront-api`.

## Services

- `ProductsService`
- `CartService`
- `CheckoutService`

## Dapr App IDs

- `products-service`
- `cart-service`
- `checkout-service`

## Local Run Ports (recommended)

- Products app port: `8081`, Dapr HTTP: `3501`
- Cart app port: `8082`, Dapr HTTP: `3502`
- Checkout app port: `8083`, Dapr HTTP: `3503`

## Shared Dapr Components

All microservices should use component files from:

- `../../DaprAcaStarter/components`

## Build

```powershell
dotnet build
```

or from repo root:

```powershell
dotnet build "Dapr ACA.sln"
```
