resource "google_storage_bucket_iam_member" "public-read" {
  bucket = var.bucket
  role   = "roles/storage.objectViewer"
  member = "allUsers"
}

resource "google_storage_bucket_iam_member" "group-admin" {
  bucket = var.bucket
  role   = "roles/storage.objectAdmin"
  member = "group:${var.access_group_email}"
  count  = "${var.access_group_email}" != "none" ? 1 : 0
}

resource "google_storage_bucket_iam_member" "webapi-admin" {
  bucket = var.bucket
  role   = "roles/storage.objectAdmin"
  member = "serviceAccount:${google_service_account.app.email}"
}
