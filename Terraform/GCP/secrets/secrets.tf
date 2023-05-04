resource "google_secret_manager_secret" "secret_es_api" {
  secret_id = "es-api"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret_es_api" {
  secret      = google_secret_manager_secret.secret_es_api.id
  secret_data = var.es_api_pass
}

resource "google_secret_manager_secret" "secret_app_pass" {
  secret_id = "app-pass"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret_app_pass" {
  secret      = google_secret_manager_secret.secret_app_pass.id
  secret_data = var.sql_api_pass
}

resource "google_secret_manager_secret" "secret_auth_pass" {
  secret_id = "auth-pass"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret_auth_pass" {
  secret      = google_secret_manager_secret.secret_auth_pass.id
  secret_data = var.sql_auth_pass
}

resource "google_secret_manager_secret" "secret_sendgrid_key" {
  secret_id = "sendgrid-key"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "secret_sendgrid_key" {
  secret      = google_secret_manager_secret.secret_sendgrid_key.id
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

resource "google_secret_manager_secret" "kube_secret" {
  secret_id = "kubeconfig"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "kube_secret" {
  secret      = google_secret_manager_secret.kube_secret.id
  secret_data = var.deployer_kubeconfig
}

locals {
  api_list          = split("/", google_secret_manager_secret_version.secret_app_pass.name)
  auth_list         = split("/", google_secret_manager_secret_version.secret_auth_pass.name)
  es_api_list       = split("/", google_secret_manager_secret_version.secret_es_api.name)
  sendgrid_key_list = split("/", google_secret_manager_secret_version.secret_sendgrid_key.name)
  redis_list        = split("/", google_secret_manager_secret_version.redis_secret.name)
}
