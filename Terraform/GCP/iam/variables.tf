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

variable "ssh_user" {
  type        = string
  description = "SSH User for instance"
}

variable "ssh_key" {
  type        = string
  description = "SSH Public Key for instance"
}
