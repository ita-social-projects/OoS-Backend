variable "app_sa_email" {
  type = string
}

variable "auth_sa_email" {
  type = string
}

variable "front_sa_email" {
  type = string
}

variable "api_secret" {
  type = string
}

variable "auth_secret" {
  type = string
}

variable "mongo_secret" {
  type = string
}

variable "es_api_pass_secret" {
  type = string
}

variable "project" {
  type        = string
  description = "Your project"
}

variable "zone" {
  type        = string
  description = "Zone to create the resources in"
}

variable "region" {
  type        = string
  description = "Region to create the resources in"
}
