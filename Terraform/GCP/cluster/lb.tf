resource "google_compute_address" "lb" {
  name = "k3s-static-${var.random_number}"
}

resource "google_compute_region_backend_service" "k3s" {
  provider              = google-beta
  project               = var.project
  region                = var.region
  name                  = "k3s-api-${var.random_number}"
  health_checks         = [google_compute_region_health_check.k3s.id]
  protocol              = "TCP"
  load_balancing_scheme = "EXTERNAL"

  dynamic "backend" {
    for_each = module.master
    content {
      group          = backend.value["mig_url"]
      balancing_mode = "CONNECTION"
    }
  }
}
resource "google_compute_region_health_check" "k3s" {
  provider = google-beta
  project  = var.project
  region   = var.region
  name     = "k3s-health-check-${var.random_number}"

  timeout_sec         = 5
  check_interval_sec  = 10
  healthy_threshold   = 1
  unhealthy_threshold = 3

  ssl_health_check {
    port         = "6443"
    proxy_header = "NONE"
  }
}

resource "google_compute_forwarding_rule" "k8s-api" {
  name                  = "k3s-api-${var.random_number}"
  backend_service       = google_compute_region_backend_service.k3s.id
  load_balancing_scheme = "EXTERNAL"
  port_range            = "6443"
  ip_address            = google_compute_address.lb.address
  ip_protocol           = "TCP"
}
