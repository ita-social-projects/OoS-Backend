resource "google_storage_bucket" "logs" {
  name          = "oos-logs-${var.random_number}"
  location      = var.region
  force_destroy = true

  uniform_bucket_level_access = true
  storage_class               = "REGIONAL"

  lifecycle_rule {
    action {
      type = "Delete"
    }

    condition {
      age = 31
    }
  }

  retention_policy {
    is_locked        = false
    retention_period = 2592000
  }
}
