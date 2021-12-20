resource "google_secret_manager_secret" "secret-es-api" {
  secret_id = "es-api"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret-es-api" {
  secret      = google_secret_manager_secret.secret-es-api.id
  secret_data = var.es_api_pass
}

resource "google_secret_manager_secret" "secret-app-connection" {
  secret_id = "app-connection"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret-app-connection" {
  secret      = google_secret_manager_secret.secret-app-connection.id
  secret_data = "server=${var.sql_hostname};user=api;password=${var.sql_api_pass};database=outofschool;guidformat=binary16"
}

resource "google_secret_manager_secret" "secret-auth-connection" {
  secret_id = "auth-connection"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret-auth-connection" {
  secret      = google_secret_manager_secret.secret-auth-connection.id
  secret_data = "server=${var.sql_hostname};user=oos;password=${var.sql_auth_pass};database=outofschool;guidformat=binary16"
}

resource "google_secret_manager_secret" "secret-mongo-connection" {
  secret_id = "mongo-connection"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret-mongo-connection" {
  secret      = google_secret_manager_secret.secret-mongo-connection.id
  secret_data = "mongodb://oos:${var.mongo_pass}@${var.mongo_hostname}:27017/outofschool?authSource=outofschool"
}

locals {
  api_list    = split("/", google_secret_manager_secret_version.secret-app-connection.name)
  auth_list   = split("/", google_secret_manager_secret_version.secret-auth-connection.name)
  es_api_list = split("/", google_secret_manager_secret_version.secret-es-api.name)
  mongo_list  = split("/", google_secret_manager_secret_version.secret-mongo-connection.name)
}
