variable "project" {
  type        = string
  description = "Your project"
}

variable "zone" {
  type        = string
  description = "Zone where the cluster was created"
}

variable "admin_ips" {
  type        = list(string)
  description = "Admin IPs to manage database if needed"
}

variable "sql_api_pass" {
  type = string
}

variable "sql_auth_pass" {
  type = string
}

variable "sql_root_pass" {
  type = string
}

variable "es_admin_pass" {
  type = string
}

variable "es_api_pass" {
  type = string
}

variable "csi_sa_email" {
  type = string
}

variable "csi_sa_key" {
}

variable "letsencrypt_email" {
  type        = string
  description = "E-mail of letsencrypt user"
}

variable "mongo_root_pass" {
  type = string
}

variable "mongo_pass" {
  type = string
}

variable "sql_hostname" {
  type        = string
  description = "Hostname for application database"
}

variable "mongo_hostname" {
  type        = string
  description = "Hostname for application MongoDB"
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