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

resource "google_secret_manager_secret" "secret-sendgrid-key" {
  secret_id = "sendgrid-key"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret-sendgrid-key" {
  secret      = google_secret_manager_secret.secret-sendgrid-key.id
  secret_data = var.sendgrid_key
}

resource "google_secret_manager_secret" "redis_secret" {
  secret_id = "redis-pass"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "redis_secret" {
  secret      = google_secret_manager_secret.redis_secret.id
  secret_data = var.redis_pass
}

locals {
  api_list          = split("/", google_secret_manager_secret_version.secret-app-connection.name)
  auth_list         = split("/", google_secret_manager_secret_version.secret-auth-connection.name)
  es_api_list       = split("/", google_secret_manager_secret_version.secret-es-api.name)
  sendgrid_key_list = split("/", google_secret_manager_secret_version.secret-sendgrid-key.name)
  redis_list        = split("/", google_secret_manager_secret_version.redis_secret.name)
}
