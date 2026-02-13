param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $true)]
    [string]$ContainerAppsEnvironment,

    [Parameter(Mandatory = $false)]
    [switch]$RemoveLegacyApp
)

$ErrorActionPreference = 'Stop'

if ($RemoveLegacyApp) {
    Write-Host 'Deleting legacy container app: dapr-aca-starter'
    az containerapp delete -g $ResourceGroup -n dapr-aca-starter -y
}

Write-Host 'Applying Dapr components (statestore, pubsub)...'
az containerapp env dapr-component set -g $ResourceGroup -n $ContainerAppsEnvironment --dapr-component-name statestore --yaml ./statestore.aca.yaml
az containerapp env dapr-component set -g $ResourceGroup -n $ContainerAppsEnvironment --dapr-component-name pubsub --yaml ./pubsub.aca.yaml

Write-Host 'Deploying microservices container apps...'
az containerapp up -g $ResourceGroup --yaml ./containerapp.products.yaml
az containerapp up -g $ResourceGroup --yaml ./containerapp.cart.yaml
az containerapp up -g $ResourceGroup --yaml ./containerapp.checkout.yaml
az containerapp up -g $ResourceGroup --yaml ./containerapp.dapr.yaml

Write-Host 'Deployment finished.'
