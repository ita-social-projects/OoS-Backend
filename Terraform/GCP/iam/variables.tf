variable "random_number" {
  type = number
}

variable "access_group_email" {
  type = string
}

variable "project" {
  type        = string
  description = "Your project"
}

variable "bucket" {
  type = string
}

variable "logs_bucket" {
  type = string
}

variable "devops" {
  type        = list(string)
  description = "E-mails of devops with edit permissions"
}
