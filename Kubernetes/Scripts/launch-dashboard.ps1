#az login
az account set --subscription "LunchBag"
Write-Host "Use Url http://localhost:8001/api/v1/namespaces/kube-system/services/kubernetes-dashboard/proxy/#"
az aks get-credentials --resource-group lunchbag --name lunchbagakscluster
#az acs kubernetes get-credentials --resource-group lunchbag --name lunchbagcluster
kubectl proxy