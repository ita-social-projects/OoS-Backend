output "image_bucket" {
  value = google_storage_bucket.images.name
}

output "logs_bucket" {
  value = google_storage_bucket.logs.name
}