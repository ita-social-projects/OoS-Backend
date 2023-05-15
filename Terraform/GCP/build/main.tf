resource "google_pubsub_topic" "cloud_build" {
  name = "cloud-builds"
}

resource "google_pubsub_topic" "gcr" {
  name = "gcr"
}

resource "google_cloudbuild_trigger" "backend_api" {
  name = "backend-api"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Backend"
    push {
      branch = "develop"
    }
  }
  substitutions = {
    _ASPNETCORE_ENVIRONMENT = "Google"
    _ZONE                   = var.zone
    _REGION                 = var.region
    _SERVICE_ACCOUNT        = var.app_sa_email
    _DB_PASS                = var.api_secret
    _ES_PASSWORD            = var.es_api_pass_secret
    _BUCKET                 = var.bucket
    _REDIS_HOST             = var.redis_hostname
    _REDIS_PASS             = var.redis_secret
    _REDIS_PORT             = var.redis_port
    _SQL_PORT               = var.sql_port
    _GEO_KEY                = var.geo_key_secret
  }

  filename = "cloudbuild-app.yml"
}

resource "google_cloudbuild_trigger" "backend_auth" {
  name = "backend-auth"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Backend"
    push {
      branch = "develop"
    }
  }

  filename = "cloudbuild-auth.yml"
}

resource "google_cloudbuild_trigger" "auth_deploy" {
  name = "backend-auth-deploy"
  pubsub_config {
    topic = google_pubsub_topic.gcr.id
  }

  substitutions = {
    _KUBE_CONFIG  = var.kube_secret
    _POOL         = google_cloudbuild_worker_pool.pool.id
    _ACTION       = "$(body.message.data.action)"
    _IMAGE_TAG    = "$(body.message.data.tag)"
    _HOST         = var.auth_hostname
    _SENDER_EMAIL = var.sender_email
    _SERVICE_NAME = "authserver"
  }

  source_to_build {
    uri       = "https://github.com/ita-social-projects/OoS-Backend"
    ref       = "refs/heads/develop"
    repo_type = "GITHUB"
  }

  git_file_source {
    path      = "cloudbuild-auth-deploy.yaml"
    uri       = "https://github.com/ita-social-projects/OoS-Backend"
    revision  = "refs/heads/develop"
    repo_type = "GITHUB"
  }
  filter = "_ACTION.matches(\"INSERT\") && _IMAGE_TAG.matches(\"^.*oos-auth:.*$\")"
}
