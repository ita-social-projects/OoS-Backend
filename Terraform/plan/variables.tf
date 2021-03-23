variable "location" {
  type        = string
  description = "Azure Location"
}

variable "rg_name" {
  type        = string
  description = "Azure Resource Group Name"
}

variable "plan" {
  type        = any
  default     = {}
  description = "App Service plan properties."
}

variable "tags" {
  type        = map(string)
  description = "A mapping of tags to assign to the resources."
}
