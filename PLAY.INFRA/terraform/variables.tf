variable "cluster_name" {
  type = string
  default = "playeconomy_cluster"
}

variable "location" {
  type = string
  default = "Central India"
}

variable "node_count" {
  type = number
  default = 2
}

variable "subscription_id" {
  type = string
  default = "value"
}

variable "client_id" {
    type = string
    default = "value"
}

variable "azurerm_resource_group_name" {
  type = string
  default   = "bonganelebopo"
}

variable "azurerm_resource_group_location" {
  type = string
  default = "centralindia"
}