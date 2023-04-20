variable "project" {
  type        = string
  description = "Your GCP Project"
}

variable "region" {
  type        = string
  description = "Region to create the resources in"
}

variable "random_number" {
  type = number
}

variable "ingress_name" {
  type = string
}

variable "mig_url" {
  type = string
}

variable "tags" {
  type        = list(string)
  description = "A list of network tags to assign to the resources."
  default     = ["mysql", "elastic"]
}

variable "network_name" {
  type = string
}

variable "cloud_run_lb" {
  type = object({
    enable = bool
    # Run load balancer on HTTPS and provision managed certificate with provided `domain`.
    ssl = bool
    # Domain names to run the load balancer on. Used if `ssl` is `true`.
    services = map(
      object({
        domain = string
      })
    )
  })
}
