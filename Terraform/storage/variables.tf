variable "location" {
  type        = string
  description = "Azure Location"
}

variable "rg_name" {
  type        = string
  description = "Azure Resource Group Name"
}

variable "subnet_id" {
  description = "Azure Subnet ID"
}

variable "random_number" {
  type = number
}

variable "tags" {
  type        = map(string)
  description = "A mapping of tags to assign to the resources."
}