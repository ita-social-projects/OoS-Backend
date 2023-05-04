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

variable "tags" {
  type        = list(string)
  description = "A list of network tags to assign to the resources."
  default     = ["mysql", "elastic"]
}

variable "labels" {
  type        = map(string)
  description = "A mapping of labels to assign to the resources."
}

variable "machine_type" {
  type        = map(any)
  description = "Instance type used for instances"
  default = {
    "n1micro"     = "f1-micro"      #1vCPU 0.6GB RAM
    "n1small"     = "g1-small"      #1vCPU 1.7GB RAM
    "e2micro"     = "e2-micro"      #2vCPU 1.0GB RAM
    "e2small"     = "e2-small"      #2vCPU 2.0GB RAM
    "e2medium"    = "e2-medium"     #2vCPU 4.0GB RAM
    "n1standard1" = "n1-standard-1" #1vCPU 3.75GB RAM
    "n1standard2" = "n1-standard-2" #2vCPU 7.5GB RAM
    "n1standard4" = "n1-standard-4" #4vCPU 15GB RAM
  }
}

variable "shutdown" {
  description = "Shutdown Script"
}
variable "startup" {
  description = "Startup Script"
}

variable "node_role" {
  type        = string
  description = "Node role"
  validation {
    condition     = contains(["master", "worker"], var.node_role)
    error_message = "Valid value is one of the following: master, worker."
  }
}

variable "node_count" {
  description = "Number of Nodes"
  type        = number
}

variable "network_name" {
  type = string
}
