# resource "google_cloudbuild_trigger" "frontend_release" {
#   name = "frontend-release"
#   pubsub_config {
#     topic = google_pubsub_topic.cloud_build.id
#   }

#   substitutions = {
#     _GITHUB_CLI_VERSION = "2.11.3"
#     _GITHUB_TOKEN       = var.github_token_secret
#     _BUILD_STATUS       = "$(body.message.data.status)"
#     _BUILD_TRIGGER_ID   = "$(body.message.data.buildTriggerId)"
#   }

#   source_to_build {
#     uri       = "https://github.com/ita-social-projects/OoS-Frontend"
#     ref       = "refs/heads/main"
#     repo_type = "GITHUB"
#   }

#   git_file_source {
#     path      = "cloudbuild-stage.yaml"
#     uri       = "https://github.com/ita-social-projects/OoS-Frontend"
#     revision  = "refs/heads/main"
#     repo_type = "GITHUB"
#   }
#   filter = "_BUILD_STATUS.matches(\"SUCCESS\") && _BUILD_TRIGGER_ID.matches(\"${element(local.frontend_stage_trigger_id, length(local.frontend_stage_trigger_id) - 1)}\")"
# }

# resource "google_cloudbuild_trigger" "backend_api" {
#   name = "backend-api"
#   github {
#     owner = "ita-social-projects"
#     name  = "OoS-Backend"
#     push {
#       tag = "gcp-[0-9]\\.[0-9]\\.[0-9]"
#     }
#   }
#   substitutions = {
#     _ASPNETCORE_ENVIRONMENT = "Google"
#     _ZONE                   = var.zone
#     _REGION                 = var.region
#     _SERVICE_ACCOUNT        = var.app_sa_email
#     _DB_PASS                = var.api_secret
#     _ES_PASSWORD            = var.es_api_pass_secret
#     _BUCKET                 = var.bucket
#     _REDIS_HOST             = var.redis_hostname
#     _REDIS_PASS             = var.redis_secret
#     _REDIS_PORT             = var.redis_port
#     _SQL_PORT               = var.sql_port
#   }

#   filename = "cloudbuild-app.yml"
# }

# resource "google_cloudbuild_trigger" "backend_auth" {
#   name = "backend-auth"
#   github {
#     owner = "ita-social-projects"
#     name  = "OoS-Backend"
#     push {
#       tag = "gcp-[0-9]\\.[0-9]\\.[0-9]"
#     }
#   }

#   substitutions = {
#     _ASPNETCORE_ENVIRONMENT = "Google"
#     _REGION                 = var.region
#     _SERVICE_ACCOUNT        = var.auth_sa_email
#     _DB_PASS                = var.auth_secret
#     _SENDER_EMAIL           = var.sender_email
#     _SENDGRID_KEY           = var.sendgrid_key_secret
#     _SQL_PORT               = var.sql_port
#   }

#   filename = "cloudbuild-auth.yml"
# }

locals {
  frontend_stage_trigger_id = split("/", google_cloudbuild_trigger.frontend_stage.id)
}
