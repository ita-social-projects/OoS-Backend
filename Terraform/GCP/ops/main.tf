resource "google_compute_project_metadata" "os_login" {
  metadata = {
    block-project-ssh-keys = true
    enable-oslogin         = "TRUE"
  }
}

resource "google_dns_policy" "dns_logging" {
  name = "dns-logging-policy-${var.random_number}"

  enable_logging = true

  networks {
    network_url = var.network_id
  }
}
