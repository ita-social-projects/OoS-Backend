variable "project" {
  type        = string
  description = "Your GCP Project"
}

variable "region" {
  type        = string
  description = "Region to create the resources in"
}

variable "zone" {
  type        = string
  description = "Zone to create the resources in"
}

variable "credentials" {
  type        = string
  description = "Path to GCP Service Account key JSON"
}

variable "labels" {
  type        = map(string)
  default     = {}
  description = "A mapping of labels to assign to the resources."
}

variable "access_group_email" {
  type        = string
  default     = "none"
  description = "Google Group that will receive access permissions"
}

variable "admin_ips" {
  type        = list(string)
  default     = []
  description = "Admin IPs to manage database if needed"
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

variable "ssh_user" {
  type        = string
  description = "SSH User for instance"
}

variable "ssh_key" {
  type        = string
  description = "SSH Public Key for instance"
}

variable "letsencrypt_email" {
  type        = string
  description = "E-mail of letsencrypt user"
}

variable "sql_hostname" {
  type        = string
  description = "Hostname for application database"
}

variable "mongo_hostname" {
  type        = string
  description = "Hostname for application MongoDB"
}

variable "k8s_api_hostname" {
  type        = string
  description = "Hostname for K8S API"
}
