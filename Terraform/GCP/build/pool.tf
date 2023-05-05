resource "google_cloudbuild_worker_pool" "pool" {
  name     = "private-pool-${var.random_number}"
  location = var.region
  worker_config {
    machine_type   = "e2-medium"
    no_external_ip = false
    disk_size_gb   = 100
  }
  network_config {
    peered_network          = var.network_id
    peered_network_ip_range = "/29"
  }
}

resource "google_compute_firewall" "pool" {
  project = var.project
  name    = "fw-allow-private-pool-${var.random_number}"
  network = var.network_id

  direction = "INGRESS"

  allow {
    protocol = "tcp"
    ports    = ["6443"]
  }

  source_ranges = [var.private_ip_range]
  target_tags   = var.tags
}
