resource "google_service_account" "csi" {
  account_id   = "csi-${var.random_number}"
  display_name = "CSI Driver Service Account"
}

resource "time_rotating" "csi_key_rotation" {
  rotation_days = 30
}

resource "google_service_account_key" "csi" {
  service_account_id = google_service_account.csi.name

  keepers = {
    rotation_time = time_rotating.csi_key_rotation.rotation_rfc3339
  }
}

data "google_iam_role" "compute_storage" {
  name = "roles/compute.storageAdmin"
}

resource "google_project_iam_custom_role" "csi_storage" {
  role_id     = "gcp_compute_storage_admin_custom_role"
  title       = "Custom Compute Storage Admin"
  description = "Role with filtered resourcemanager.projects.get & resourcemanager.projects.list for Prisma Cloud Exception"
  permissions = [
    for p in data.google_iam_role.compute_storage.included_permissions : p if length(regexall("resourcemanager.projects.*", p)) == 0
  ]
}

data "google_iam_role" "csi_sauser" {
  name = "roles/iam.serviceAccountUser"
}

resource "google_project_iam_custom_role" "csi_sauser" {
  role_id     = "gcp_sa_user_custom_role"
  title       = "Custom Service Account User"
  description = "Role with filtered resourcemanager.projects.get & resourcemanager.projects.list for Prisma Cloud Exception"
  permissions = [
    for p in data.google_iam_role.csi_sauser.included_permissions : p if length(regexall("resourcemanager.projects.*", p)) == 0
  ]
}

resource "google_project_iam_custom_role" "csi" {
  role_id     = "gcp_compute_persistent_disk_csi_driver_custom_role"
  title       = "Custom CSI Role"
  description = "A description"
  permissions = [
    "compute.instances.get",
    "compute.instances.attachDisk",
    "compute.instances.detachDisk"
  ]
}

resource "google_project_iam_member" "gke-csi" {
  role    = google_project_iam_custom_role.csi.id
  member  = "serviceAccount:${google_service_account.csi.email}"
  project = var.project
}

resource "google_project_iam_member" "csi_storage" {
  role    = google_project_iam_custom_role.csi_storage.id
  member  = "serviceAccount:${google_service_account.csi.email}"
  project = var.project
}

resource "google_project_iam_member" "csi_sauser" {
  role    = google_project_iam_custom_role.csi_sauser.id
  member  = "serviceAccount:${google_service_account.csi.email}"
  project = var.project
}
