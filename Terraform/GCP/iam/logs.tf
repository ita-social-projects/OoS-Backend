resource "google_storage_bucket_iam_member" "logs_access" {
  bucket = var.logs_bucket
  role   = "roles/storage.legacyBucketWriter"
  member = "group:cloud-storage-analytics@google.com"
}
