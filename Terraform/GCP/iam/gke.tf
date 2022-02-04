resource "google_service_account" "gke" {
  account_id   = "gke-instance-${var.random_number}"
  display_name = "GKE Service Account"
}

resource "google_project_iam_member" "gke-bucket" {
  role   = "roles/storage.objectViewer"
  member = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

resource "google_project_iam_member" "gke-trace" {
  role   = "roles/cloudtrace.agent"
  member = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

resource "google_project_iam_member" "gke-log" {
  role   = "roles/logging.logWriter"
  member = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

resource "google_project_iam_member" "gke-metrics" {
  role   = "roles/monitoring.metricWriter"
  member = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

# TODO: Check if needed for GCP CCM
resource "google_project_iam_member" "gke-compute" {
  role   = "roles/compute.admin"
  member = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

resource "google_project_iam_member" "gke-lb" {
  role   = "roles/compute.loadBalancerAdmin"
  member = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}
