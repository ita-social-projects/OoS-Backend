variable "location" {
  type        = string
  description = "Azure Location"
}

variable "rg_name" {
  type        = string
  description = "Azure Resource Group Name"
}

variable "random_number" {
  type = number
}

variable "tags" {
  type        = map(string)
  description = "A mapping of tags to assign to the resources."
}