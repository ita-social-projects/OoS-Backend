resource "google_cloudbuild_trigger" "backend_tag" {
  name = "backend-tag"
  pubsub_config {
    topic = google_pubsub_topic.cloud_build.id
  }

  substitutions = {
    _GITHUB_DEPLOY    = var.github_back_secret
    _BUILD_STATUS     = "$(body.message.data.status)"
    _BUILD_TRIGGER_ID = "$(body.message.data.buildTriggerId)"
  }

  source_to_build {
    uri       = "https://github.com/ita-social-projects/OoS-Backend"
    ref       = "refs/heads/develop"
    repo_type = "GITHUB"
  }

  git_file_source {
    path      = "cloudbuild-tag.yaml"
    uri       = "https://github.com/ita-social-projects/OoS-Backend"
    revision  = "refs/heads/develop"
    repo_type = "GITHUB"
  }
  filter = "_BUILD_STATUS.matches(\"SUCCESS\") && (_BUILD_TRIGGER_ID.matches(\"${element(local.api_trigger_id, length(local.api_trigger_id) - 1)}\") || _BUILD_TRIGGER_ID.matches(\"${element(local.auth_trigger_id, length(local.auth_trigger_id) - 1)}\"))"
}

resource "google_cloudbuild_trigger" "frontend_tag" {
  name = "frontend-tag"
  pubsub_config {
    topic = google_pubsub_topic.cloud_build.id
  }

  substitutions = {
    _GITHUB_DEPLOY    = var.github_front_secret
    _BUILD_STATUS     = "$(body.message.data.status)"
    _BUILD_TRIGGER_ID = "$(body.message.data.buildTriggerId)"
  }

  source_to_build {
    uri       = "https://github.com/ita-social-projects/OoS-Frontend"
    ref       = "refs/heads/develop"
    repo_type = "GITHUB"
  }

  git_file_source {
    path      = "cloudbuild-tag.yaml"
    uri       = "https://github.com/ita-social-projects/OoS-Frontend"
    revision  = "refs/heads/develop"
    repo_type = "GITHUB"
  }
  filter = "_BUILD_STATUS.matches(\"SUCCESS\") && _BUILD_TRIGGER_ID.matches(\"${element(local.frontend_trigger_id, length(local.frontend_trigger_id) - 1)}\")"
}

locals {
  api_trigger_id      = split("/", google_cloudbuild_trigger.backend_api.id)
  auth_trigger_id     = split("/", google_cloudbuild_trigger.backend_auth.id)
  frontend_trigger_id = split("/", google_cloudbuild_trigger.frontend.id)
}
