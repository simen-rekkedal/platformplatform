variable "tags" {
  description = "Azure tags put on all resource to en able better understanding owner and billing."
  type = object({
    environment = string
    managed-by  = string
  })
}

variable "environment" {
  description = "The environment used for billing insighs. E.g. development, staging, production, shared."
  type        = string
}

variable "resource_location" {
  description = "The location of resources."
  type        = string
}

variable "resource_group_id" {
  description = "The name of the recource group."
  type        = string
}

variable "subnet_id" {
  description = "Id of the subnet that have access to the Key Vault."
  type        = string
}
