resource "google_cloudbuild_trigger" "frontend_release" {
  name = "frontend-release"
  pubsub_config {
    topic = google_pubsub_topic.cloud_build.id
  }

  substitutions = {
    _GITHUB_CLI_VERSION = "2.11.3"
    _GITHUB_TOKEN       = var.github_token_secret
    _BUILD_STATUS       = "$(body.message.data.status)"
    _BUILD_TRIGGER_ID   = "$(body.message.data.buildTriggerId)"
  }

  source_to_build {
    uri       = "https://github.com/ita-social-projects/OoS-Frontend"
    ref       = "refs/heads/main"
    repo_type = "GITHUB"
  }

  git_file_source {
    path      = "cloudbuild-stage.yaml"
    uri       = "https://github.com/ita-social-projects/OoS-Frontend"
    revision  = "refs/heads/main"
    repo_type = "GITHUB"
  }
  filter = "_BUILD_STATUS.matches(\"SUCCESS\") && _BUILD_TRIGGER_ID.matches(\"${element(local.frontend_stage_trigger_id, length(local.frontend_stage_trigger_id) - 1)}\")"
}

locals {
  frontend_stage_trigger_id = split("/", google_cloudbuild_trigger.frontend_stage.id)
}
