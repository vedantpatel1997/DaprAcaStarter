# ACA Manifests (`DaprAcaStarter/aca`)

This folder contains deployment templates and scripts for Azure Container Apps.

## Files

- `containerapp.dapr.yaml`: storefront API (`storefront-api`)
- `containerapp.products.yaml`: products microservice
- `containerapp.cart.yaml`: cart microservice
- `containerapp.checkout.yaml`: checkout microservice
- `statestore.aca.yaml`: Dapr state component (Redis)
- `pubsub.aca.yaml`: Dapr pub/sub component (Redis)
- `deploy-microservices.ps1`: helper deploy script

## Pre-Deployment Checklist

1. Ensure images already exist in ACR.
2. Replace placeholders in YAML:
   - `<SUBSCRIPTION_ID>`
   - `<RESOURCE_GROUP>`
   - `<ACA_ENV_NAME>`
   - `<ACR_LOGIN_SERVER>`
3. Confirm Dapr components point to reachable Redis host in ACA.

## Deploy Dapr Components

```powershell
az containerapp env dapr-component set -g <RG> -n <ACA_ENV_NAME> --dapr-component-name statestore --yaml ./statestore.aca.yaml
az containerapp env dapr-component set -g <RG> -n <ACA_ENV_NAME> --dapr-component-name pubsub --yaml ./pubsub.aca.yaml
```

## Deploy Apps

```powershell
az containerapp create -g <RG> --yaml ./containerapp.products.yaml
az containerapp create -g <RG> --yaml ./containerapp.cart.yaml
az containerapp create -g <RG> --yaml ./containerapp.checkout.yaml
az containerapp create -g <RG> --yaml ./containerapp.dapr.yaml
```

For existing apps:

```powershell
az containerapp update -g <RG> --yaml ./containerapp.products.yaml
az containerapp update -g <RG> --yaml ./containerapp.cart.yaml
az containerapp update -g <RG> --yaml ./containerapp.checkout.yaml
az containerapp update -g <RG> --yaml ./containerapp.dapr.yaml
```

## Optional Helper Script

```powershell
.\deploy-microservices.ps1 -ResourceGroup <RG> -ContainerAppsEnvironment <ACA_ENV_NAME>
```

To remove old app `dapr-aca-starter` first:

```powershell
.\deploy-microservices.ps1 -ResourceGroup <RG> -ContainerAppsEnvironment <ACA_ENV_NAME> -RemoveLegacyApp
```
