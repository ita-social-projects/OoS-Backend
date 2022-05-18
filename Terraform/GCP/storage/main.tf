resource "google_storage_bucket" "images" {
  name          = "oos-images-${var.random_number}"
  location      = var.region
  force_destroy = true

  uniform_bucket_level_access = true
  storage_class               = "REGIONAL"
}
