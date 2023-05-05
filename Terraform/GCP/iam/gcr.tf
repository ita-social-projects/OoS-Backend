resource "google_service_account" "pull" {
  account_id   = "gcr-puller-${var.random_number}"
  display_name = "Pull from Container Registry"
}

resource "google_storage_bucket_iam_member" "pull" {
  for_each = local.prefixes
  bucket   = "${each.key}artifacts.${var.project}.appspot.com"
  role     = "roles/storage.objectViewer"
  member   = "serviceAccount:${google_service_account.pull.email}"
}

resource "time_rotating" "pull_key_rotation" {
  rotation_days = 30
}

resource "google_service_account_key" "pull" {
  service_account_id = google_service_account.pull.name

  keepers = {
    rotation_time = time_rotating.pull_key_rotation.rotation_rfc3339
  }
}

locals {
  prefixes = toset(["", "eu.", "us."])
}
