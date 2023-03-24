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

variable "redis_pass" {
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

variable "sql_hostname" {
  type        = string
  description = "Hostname for application database"
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

variable "sql_port" {
  type = number
}

variable "redis_port" {
  type = number
}

variable "enable_ingress_http" {
  type = bool
}

variable "vector_logs_name" {
  type    = string
  default = "vector"
}

variable "vector_logs_chart_path" {
  type    = string
  default = "../../k8s/elastic"
}

variable "vector_logs_namespace" {
  type    = string
  default = "vector"
}

variable "es_admin_user" {
  type    = string
  default = "elastic"
}

variable "elastic_url" {
  type    = string
  default = "http://elasticsearch-master.default.svc.cluster.local:9200"
}

