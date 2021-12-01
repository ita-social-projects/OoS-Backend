# TODO: maybe change to roles/cloudbuild.builds.editor
resource "google_project_iam_member" "build-viewer" {
  role   = "roles/cloudbuild.builds.viewer"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}

resource "google_project_iam_member" "logs-viewer" {
  role   = "roles/logging.viewer"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}

resource "google_project_iam_member" "run-viewer" {
  role   = "roles/run.viewer"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}

resource "google_project_iam_member" "monitoring-viewer" {
  role   = "roles/monitoring.viewer"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}

resource "google_project_iam_member" "error-viewer" {
  role   = "roles/errorreporting.viewer"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}

resource "google_project_iam_member" "compute-viewer" {
  role   = "roles/compute.viewer"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}

resource "google_project_iam_member" "trace-user" {
  role   = "roles/cloudtrace.user"
  member = "group:${var.access_group_email}"
  count  = var.access_group_email == "none" ? 0 : 1
}
