variable "sql_api_pass" {
  type = string
}

variable "sql_auth_pass" {
  type = string
}

variable "es_api_pass" {
  type = string
}

variable "labels" {
  type        = map(string)
  description = "A mapping of labels to assign to the resources."
}

variable "sql_hostname" {
  type = string
}

variable "mongo_root_pass" {
  type = string
}

variable "mongo_pass" {
  type = string
}
