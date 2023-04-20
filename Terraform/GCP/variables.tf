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

variable "devops" {
  type        = list(string)
  description = "E-mails of devops with edit permissions"
}

variable "letsencrypt_email" {
  type        = string
  description = "E-mail of letsencrypt user"
}

variable "sql_hostname" {
  type        = string
  description = "Hostname for application database"
}

variable "k8s_api_hostname" {
  type        = string
  description = "Hostname for K8S API"
}

variable "phpmyadmin_hostname" {
  type        = string
  description = "Hostname for PHPMyAdmin"
}

variable "kibana_hostname" {
  type        = string
  description = "Hostname for Kibana"
}

variable "elastic_hostname" {
  type        = string
  description = "Hostname for Elastic"
}

variable "redis_hostname" {
  type        = string
  description = "Hostname for Redis"
}

variable "sender_email" {
  type        = string
  description = "Outgoing mail"
}

variable "sendgrid_key" {
  type        = string
  description = "Outgoing mail api key"
}

variable "github_front_deploy_base64" {
  type        = string
  description = "Github Deploy key"
}

variable "github_back_deploy_base64" {
  type        = string
  description = "Github Deploy key"
}

variable "github_access_token" {
  type        = string
  description = "Github Access Token to create releases"
}

variable "sql_port" {
  type = number
}

variable "redis_port" {
  type = number
}

variable "geo_apikey" {
  type = string
}

variable "enable_ingress_http" {
  type = bool
}
