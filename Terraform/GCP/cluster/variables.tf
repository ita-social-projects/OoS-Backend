variable "project" {
  type        = string
  description = "Your project"
}

variable "credentials" {
  type        = string
  description = "Path to GCP Service Account key JSON"
}

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

variable "sa_email" {
  type = string
}

variable "admin_ips" {
  type        = list(string)
  description = "Admin IPs to manage database if needed"
}

variable "tags" {
  type        = list(string)
  description = "A list of network tags to assign to the resources."
  default     = ["mysql", "elastic"]
}

variable "labels" {
  type        = map(string)
  description = "A mapping of labels to assign to the resources."
}

variable "k8s_api_hostname" {
  description = "Hostname of K8S API"
}

variable "db_username" {
  type = string
}

variable "db_password" {
  type = string
}

variable "db_host" {
  type = string
}

variable "subnet_cidr" {
  type = string
}

variable "network_name" {
  type = string
}
