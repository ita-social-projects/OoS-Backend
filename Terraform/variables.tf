variable "subscription_id" {
  description = "Your Azure Subscription ID"
}

variable "tenant_id" {
  description = "Your Azure Tenant ID"
}

variable "resource_group_name" {
  default = "moe-dev-rg"
  description = "Resource group for MOE OOS project"
}

variable "resource_group_location" {
  default = "West Europe"
  description = "Location of MOE Resources"
}

variable "failover_location" {
  default = "North Europe"
  description = "TODO: Secondary DB location"
}

variable "admin_ip" {
  type        = string
  default     = "none"
  description = "Admin IP to update database if needed"
}

variable "tags" {
  type        = map(string)
  default     = {}
  description = "A mapping of tags to assign to the resources."
}

variable "auth_hostname" {
  type        = string
  default     = "none"
  description = "Identity Server custom hostname"
}

variable "app_hostname" {
  type        = string
  default     = "none"
  description = "Application custom hostname"
}