# TODO: maybe change to roles/cloudbuild.builds.editor
locals {
  group_roles = "${var.access_group_email}" != "none" ? [
    "roles/cloudbuild.builds.viewer",
    "roles/logging.viewer",
    "roles/run.viewer",
    "roles/monitoring.viewer",
    "roles/errorreporting.viewer",
    "roles/compute.viewer",
    "roles/cloudtrace.user"
  ] : []
}

resource "google_project_iam_member" "group_roles" {
  for_each = toset(local.group_roles)
  role     = each.key
  member   = "group:${var.access_group_email}"
  project  = var.project
}

resource "google_container_registry" "registry" {
  project = var.project
  count   = "${var.access_group_email}" != "none" ? 1 : 0
}

resource "google_storage_bucket_iam_member" "viewer" {
  bucket = google_container_registry.registry.id
  role   = "roles/storage.objectViewer"
  member = "group:${var.access_group_email}"
  count  = "${var.access_group_email}" != "none" ? 1 : 0
}
