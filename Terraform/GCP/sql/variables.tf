variable "region" {
  type        = string
  description = "Region to create the resources in"
}

variable "zone" {
  type        = string
  description = "Zone to create the database in"
}

variable "random_number" {
  type = number
}
