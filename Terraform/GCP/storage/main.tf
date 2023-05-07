resource "google_storage_bucket" "images" {
  name          = "oos-images-${var.random_number}"
  location      = var.region
  force_destroy = true

  uniform_bucket_level_access = true
  storage_class               = "REGIONAL"

  logging {
    log_bucket = google_storage_bucket.logs.name
  }
}

resource "google_storage_bucket" "gcf" {
  name          = "oos-gcf-source-${var.random_number}"
  location      = var.region
  force_destroy = true

  uniform_bucket_level_access = true
  storage_class               = "REGIONAL"
  public_access_prevention    = "enforced"

  logging {
    log_bucket = google_storage_bucket.logs.name
  }
}
