resource "google_secret_manager_secret" "github_front_secret" {
  secret_id = "github-front-deploy"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "github_front_secret" {
  secret      = google_secret_manager_secret.github_front_secret.id
  secret_data = base64decode(var.github_front_deploy_base64)
}

resource "google_secret_manager_secret" "github_back_secret" {
  secret_id = "github-back-deploy"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "github_back_secret" {
  secret      = google_secret_manager_secret.github_back_secret.id
  secret_data = base64decode(var.github_back_deploy_base64)
}

resource "google_secret_manager_secret" "github_token_secret" {
  secret_id = "github-token"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "github_token_secret" {
  secret      = google_secret_manager_secret.github_token_secret.id
  secret_data = var.github_access_token
}
