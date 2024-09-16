# Microservices Deployment on AKS

This document outlines the steps required to deploy a Helm chart on an Azure Kubernetes Service (AKS) cluster.

## Prerequisites

1. **SonarQube**: Make sure have the SonarQube tool up and running (SonarQube version 8.9.10.61524).
2. **Azure Agent**: Make sure you have an azure agent configured.
3. **Azure CLI**: Make sure you have the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed and configured.
4. **kubectl**: Kubernetes command-line tool. You can install it via Azure CLI:
   ```bash
   az aks install-cli

## Helm: Ensure Helm is installed. You can install it by following the instructions on the Helm website.
AKS Cluster: A running AKS cluster. If you don't have one, you can create it using the Azure CLI:


### Step 1: Connect to Your AKS Cluster
Authenticate to your AKS cluster using Azure CLI:

    az aks get-credentials --resource-group bonganelebopo --name playeconomy_cluster
This command configures kubectl to interact with your AKS cluster.

Verify the connection by running:

    kubectl get nodes
You should see the list of worker nodes.

### Step 2: Install the Helm Chart
To install the chart, run the following command:

    helm install microservices ./microserivecs 

### Step 3: Verify the Deployment
Check the status of your Helm release:

    helm status microservices
List the resources created by the release:

    kubectl get all --namespace <namespace>
If the chart includes a service of type LoadBalancer, you can get the external IP of the service using:

    kubectl get svc --namespace <namespace>
### Step 4: Upgrade or Uninstall the Release
Upgrade the release if you need to update it:

    helm upgrade microservices ./microserivecs 
Uninstall the release:

    helm uninstall microservices

## Troubleshooting
Check Logs: If your release is not behaving as expected, check the logs of the deployed pods:

    kubectl logs <pod-name> --namespace <namespace>
Helm Error Messages: Review error messages and ensure that any required configuration values for the Helm chart are correct.

# Additional Resources
[Azure Kubernetes Service Documentation](https://learn.microsoft.com/en-us/azure/aks/)
[Helm Official Documentation](https://helm.sh/docs/intro/using_helm/)