variable "sql_api_pass" {
  type = string
}

variable "sql_auth_pass" {
  type = string
}

variable "es_api_pass" {
  type = string
}

variable "redis_pass" {
  type = string
}

variable "labels" {
  type        = map(string)
  description = "A mapping of labels to assign to the resources."
}

variable "sql_hostname" {
  type = string
}

variable "sendgrid_key" {
  type = string
}

variable "github_front_deploy_base64" {
  type = string
}

variable "github_back_deploy_base64" {
  type = string
}

variable "github_access_token" {
  type = string
}

variable "geo_apikey" {
  type = string
}

variable "deployer_kubeconfig" {
}
