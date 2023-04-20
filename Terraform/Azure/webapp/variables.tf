variable "location" {
  type        = string
  description = "Azure Location"
}

variable "rg_name" {
  type        = string
  description = "Azure Resource Group Name"
}

variable "plan" {
  type        = string
  description = "ID of plan"
}

variable "name" {
  type        = string
  description = "Application name"
}

variable "app_settings" {
  type        = map(string)
  default     = {}
  description = "Map of App Settings."
}

variable "db_connection" {
  type        = string
  description = "SQL Server connection string"
}

variable "subnet_id" {
  type        = string
  description = "Azure Private Subnet ID"
}

variable "tags" {
  type        = map(string)
  description = "A mapping of tags to assign to the resources."
}

variable "hostname" {
  type        = string
  default     = "none"
  description = "Custom hostname"
}

variable "sku_size" {
  type        = string
  description = "SKU Size"
}
