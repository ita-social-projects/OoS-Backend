variable "location" {
  type        = string
  description = "Azure Location"
}

variable "rg_name" {
  type        = string
  description = "Azure Resource Group Name"
}

variable "subnet_id" {
  type        = string
  description = "Azure Private Subnet ID"
}

variable "random_number" {
  type = number
}

variable "admin_ip" {
  type        = string
  description = "Admin IP to update database if needed"
}

variable "tags" {
  type        = map(string)
  description = "A mapping of tags to assign to the resources."
}