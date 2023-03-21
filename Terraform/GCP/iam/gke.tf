resource "google_service_account" "gke" {
  account_id   = "gke-instance-${var.random_number}"
  display_name = "GKE Service Account"
}

resource "google_project_iam_member" "gke-trace" {
  role    = "roles/cloudtrace.agent"
  member  = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

resource "google_project_iam_member" "gke-log" {
  role    = "roles/logging.logWriter"
  member  = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

resource "google_project_iam_member" "gke-metrics" {
  role    = "roles/monitoring.metricWriter"
  member  = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

# TODO: Check if needed for GCP CCM
data "google_iam_role" "gke_compute" {
  name = "roles/compute.admin"
}

# TODO: Check if needed for GCP CCM
resource "google_project_iam_custom_role" "gke_compute" {
  role_id     = "gcp_compute_admin_custom_role"
  title       = "Custom Compute Admin"
  description = "Role with filtered resourcemanager.projects.get & resourcemanager.projects.list for Prisma Cloud Exception"
  permissions = [
    for p in data.google_iam_role.gke_compute.included_permissions :
    p if length(regexall("resourcemanager.projects.*", p)) == 0 &&
    length(regexall("compute.organizations.*", p)) == 0 &&
    length(regexall("compute.firewallPolicies.*", p)) == 0 &&
    length(regexall("compute.securityPolicies.*", p)) == 0 &&
    length(regexall("compute.oslogin.*", p)) == 0
  ]
}

# TODO: Check if needed for GCP CCM
resource "google_project_iam_member" "gke_compute" {
  role    = google_project_iam_custom_role.gke_compute.id
  member  = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

data "google_iam_role" "gke_lb" {
  name = "roles/compute.loadBalancerAdmin"
}

resource "google_project_iam_custom_role" "gke_lb" {
  role_id     = "gcp_compute_loadbalancer_admin_custom_role"
  title       = "Custom Compute Loadbalancer Admin"
  description = "Role with filtered resourcemanager.projects.get & resourcemanager.projects.list for Prisma Cloud Exception"
  permissions = [
    for p in data.google_iam_role.gke_lb.included_permissions : p if length(regexall("resourcemanager.projects.*", p)) == 0
  ]
}

resource "google_project_iam_member" "gke_lb" {
  role    = google_project_iam_custom_role.gke_lb.id
  member  = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}

data "google_iam_role" "gke_object" {
  name = "roles/storage.objectViewer"
}

resource "google_project_iam_custom_role" "gke_object" {
  role_id     = "gcp_storage_object_viewer_custom_role"
  title       = "Custom StorageObject Viewer"
  description = "Role with filtered resourcemanager.projects.get & resourcemanager.projects.list for Prisma Cloud Exception"
  permissions = [
    for p in data.google_iam_role.gke_object.included_permissions : p if length(regexall("resourcemanager.projects.*", p)) == 0
  ]
}

resource "google_project_iam_member" "gke_object" {
  role    = google_project_iam_custom_role.gke_object.id
  member  = "serviceAccount:${google_service_account.gke.email}"
  project = var.project
}
