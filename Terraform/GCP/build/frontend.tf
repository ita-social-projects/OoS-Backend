resource "google_cloudbuild_trigger" "frontend" {
  name = "frontend"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Frontend"
    push {
      branch = "develop"
    }
  }

  substitutions = {
    _SERVICE_NAME    = "frontend"
    _STS_SERVER      = "https://${var.auth_hostname}"
    _API_SERVER      = "https://${var.app_hostname}"
    _GITHUB_DEPLOY   = var.github_front_secret
  }

  filename = "cloudbuild.yaml"
}

resource "google_cloudbuild_trigger" "frontend_deploy" {
  name = "frontend-deploy"
  pubsub_config {
    topic = google_pubsub_topic.gcr.id
  }

  substitutions = {
    _KUBE_CONFIG = var.kube_secret
    _POOL        = google_cloudbuild_worker_pool.pool.id
    _ACTION      = "$(body.message.data.action)"
    _IMAGE_TAG   = "$(body.message.data.tag)"
    _HOST        = var.front_hostname
  }

  source_to_build {
    uri       = "https://github.com/ita-social-projects/OoS-Frontend"
    ref       = "refs/heads/develop"
    repo_type = "GITHUB"
  }

  git_file_source {
    path      = "cloudbuild-deploy.yaml"
    uri       = "https://github.com/ita-social-projects/OoS-Frontend"
    revision  = "refs/heads/develop"
    repo_type = "GITHUB"
  }
  filter = "_ACTION.matches(\"INSERT\") && _IMAGE_TAG.matches(\"^.*oos-frontend:.*$\")"
}
