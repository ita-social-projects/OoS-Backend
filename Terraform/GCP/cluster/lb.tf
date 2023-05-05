data "google_compute_subnetwork" "outofschool" {
  name   = "outofschool"
  region = var.region
}

resource "google_compute_address" "lb_internal" {
  name         = "k3s-static-${var.random_number}-internal"
  address_type = "INTERNAL"
  purpose      = "GCE_ENDPOINT"
  region       = var.region
  subnetwork   = data.google_compute_subnetwork.outofschool.id
}

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

  backend {
    group          = module.masters.mig_url
    balancing_mode = "CONNECTION"
  }
}

resource "google_compute_region_backend_service" "k3s_internal" {
  name                  = "k3s-api-${var.random_number}-internal"
  provider              = google-beta
  project               = var.project
  region                = var.region
  load_balancing_scheme = "INTERNAL"
  health_checks         = [google_compute_health_check.k3s_internal.id]
  backend {
    group          = module.masters.mig_url
    balancing_mode = "CONNECTION"
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

resource "google_compute_health_check" "k3s_internal" {
  provider = google-beta
  project  = var.project
  name     = "k3s-health-check-${var.random_number}-internal"

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

resource "google_compute_forwarding_rule" "k3s_api_internal" {
  name                  = "k3s-api-${var.random_number}-internal"
  region                = var.region
  load_balancing_scheme = "INTERNAL"
  allow_global_access   = true
  ip_address            = google_compute_address.lb_internal.address
  backend_service       = google_compute_region_backend_service.k3s_internal.id
  ports                 = [6443]
  subnetwork            = data.google_compute_subnetwork.outofschool.self_link
}
