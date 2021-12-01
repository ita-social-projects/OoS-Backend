resource "google_compute_firewall" "health" {
  project = var.project
  name    = "fw-allow-k3s-health-checks-${var.random_number}"
  network = "default"

  direction = "INGRESS"

  allow {
    protocol = "tcp"
    ports    = ["6443"]
  }

  source_ranges = ["35.191.0.0/16", "130.211.0.0/22", "209.85.152.0/22", "209.85.204.0/22"]
  target_tags   = var.tags
}

resource "google_compute_firewall" "admin" {
  project = var.project
  name    = "fw-allow-k3s-admin-access-${var.random_number}"
  network = "default"

  direction = "INGRESS"

  allow {
    protocol = "tcp"
    ports    = ["22", "6443"]
  }

  source_ranges = var.admin_ips
  target_tags   = var.tags
  count         = length(var.admin_ips) == 0 ? 0 : 1
}
