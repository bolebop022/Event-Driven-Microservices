# Configure the Azure provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
  skip_provider_registration = true
}

data "azurerm_container_registry" "existing_acr" {
  name                = "playeconomymicroservices"
  resource_group_name = var.azurerm_resource_group_name
}


resource "azurerm_kubernetes_cluster" "playeconomy_cluster" {
  name                = "playeconomy_cluster"
  location            = var.azurerm_resource_group_location
  resource_group_name = var.azurerm_resource_group_name
  dns_prefix          = "playeconomy"
  kubernetes_version  = "1.29.7"

  default_node_pool {
    name                = "agentpool"
    node_count          = 2
    vm_size             = "Standard_D2as_v4"
    enable_auto_scaling = true
    min_count           = 2
    max_count           = 5
    max_pods            = 30
    type                = "VirtualMachineScaleSets"
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    network_policy    = "azure"
    load_balancer_sku = "standard"
  }

  sku_tier = "Free"

  tags = {
    cg_DM         = "swati.yadav@nagarro.com"
    cg_project     = "DevOps Fresher Training"
    cg_environment = "Dev"
  }
}

resource "azurerm_role_assignment" "aks_acr" {
  scope                = data.azurerm_container_registry.existing_acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.playeconomy_cluster.kubelet_identity[0].object_id
}
