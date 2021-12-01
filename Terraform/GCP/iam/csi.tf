resource "google_service_account" "csi" {
  account_id   = "csi-${var.random_number}"
  display_name = "CSI Driver Service Account"
}

resource "google_service_account_key" "csi" {
  service_account_id = google_service_account.csi.name
}

resource "google_project_iam_member" "csi-storage" {
  role   = "roles/compute.storageAdmin"
  member = "serviceAccount:${google_service_account.csi.email}"
}

resource "google_project_iam_member" "csi-sauser" {
  role   = "roles/iam.serviceAccountUser"
  member = "serviceAccount:${google_service_account.csi.email}"
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
  role   = google_project_iam_custom_role.csi.id
  member = "serviceAccount:${google_service_account.csi.email}"
}
