resource "google_secret_manager_secret" "geo_key" {
  secret_id = "geo-api-key"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "geo_key" {
  secret      = google_secret_manager_secret.geo_key.id
  secret_data = var.geo_apikey
}

locals {
  geo_key_list = split("/", google_secret_manager_secret_version.geo_key.name)
}
