resource "google_pubsub_topic" "cloud_build" {
  name = "cloud-builds"
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

  substitutions = {
    _ASPNETCORE_ENVIRONMENT = "Google"
    _REGION                 = var.region
    _SERVICE_ACCOUNT        = var.auth_sa_email
    _DB_PASS                = var.auth_secret
    _SENDER_EMAIL           = var.sender_email
    _SENDGRID_KEY           = var.sendgrid_key_secret
    _SQL_PORT               = var.sql_port
  }

  filename = "cloudbuild-auth.yml"
}
