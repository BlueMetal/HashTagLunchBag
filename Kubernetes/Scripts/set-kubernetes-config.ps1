#Azure Container Registry Configuration
$azureSubscriptionId = '00000000-0000-0000-0000-000000000000'
$acrResourceGroup = 'lunchbag'
$acrSPName = 'SP_LunchBagContainerRegistry'
$acrSPClientId = '00000000-0000-0000-0000-000000000000'
$acrSPPassword = '00000000-0000-0000-0000-000000000000'
$acrContainerRegistry = 'registry.azurecr.io'
$acrAccountEmail = 'email@company.com'

#Registering Azure Container Registry Credentials
#First create a service principal first
#az ad sp create-for-rbac --scopes /subscriptions/$azureSubscriptionId/resourcegroups/$acrResourceGroup/providers/Microsoft.ContainerRegistry/registries/$acrContainerRegistry --role Contributor --name $acrSPName
#kubectl create secret docker-registry acr-authentication --docker-server=$acrContainerRegistry --docker-email=$acrAccountEmail --docker-username=$acrSPClientId --docker-password=$acrSPPassword

#Registering Config and Secrets
Copy-Item "Config/config-lunchbagwebportal.json" -Destination "config.json"
kubectl create configmap config-lunchbagwebportal --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagwebportalapi.json" -Destination "config.json"
kubectl create configmap config-lunchbagwebportalapi --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagwebportalbuttonservice.json" -Destination "config.json"
kubectl create configmap config-lunchbagwebportalbuttonservice --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagwebportalmessageservice.json" -Destination "config.json"
kubectl create configmap config-lunchbagwebportalmessageservice --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagwebportaltransportservice.json" -Destination "config.json"
kubectl create configmap config-lunchbagwebportaltransportservice --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagwebportalbusservice.json" -Destination "config.json"
kubectl create configmap config-lunchbagwebportalbusservice --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagregistrationportalapi.json" -Destination "config.json"
kubectl create configmap config-lunchbagregistrationportalapi --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagadminportal.json" -Destination "config.json"
kubectl create configmap config-lunchbagadminportal --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Config/config-lunchbagregistrationportal.json" -Destination "config.json"
kubectl create configmap config-lunchbagregistrationportal --from-file=./config.json
Remove-Item "config.json"

Copy-Item "Secrets/common.json" -Destination "secrets.json"
kubectl create secret generic secrets-common --from-file=./secrets.json
Remove-Item "secrets.json"

Copy-Item "Secrets/basic-auth.txt" -Destination "auth"
kubectl create secret generic basic-auth --from-file=auth
Remove-Item "auth"

kubectl create secret generic rabbitmq-credentials --from-literal=Username=Admin --from-literal=Password=LunchBag1234