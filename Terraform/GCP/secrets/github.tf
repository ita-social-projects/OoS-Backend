resource "google_secret_manager_secret" "github_secret" {
  secret_id = "github-deploy"

  labels = var.labels

  replication {
    automatic = true
  }
}

resource "google_secret_manager_secret_version" "github_secret" {
  secret      = google_secret_manager_secret.github_secret.id
  secret_data = base64decode(var.github_deploy_base64)
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
