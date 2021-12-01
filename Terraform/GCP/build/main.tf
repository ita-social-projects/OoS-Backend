resource "google_cloudbuild_trigger" "backend-api" {
  name = "backend-api"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Backend"
    push {
      tag = "gcp-[0-9]\\.[0-9]\\.[0-9]"
    }
  }
  substitutions = {
    _ZONE               = var.zone
    _REGION             = var.region
    _SERVICE_ACCOUNT    = var.app_sa_email
    _CONN_STRING_SECRET = var.api_secret
    _ES_PASSWORD        = var.es_api_pass_secret
  }

  filename = "app-cloudbuild.yml"
}

resource "google_cloudbuild_trigger" "backend-auth" {
  name = "backend-auth"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Backend"
    push {
      tag = "gcp-[0-9]\\.[0-9]\\.[0-9]"
    }
  }

  substitutions = {
    _REGION             = var.region
    _SERVICE_ACCOUNT    = var.auth_sa_email
    _CONN_STRING_SECRET = var.auth_secret
  }

  filename = "auth-cloudbuild.yml"
}

resource "google_cloudbuild_trigger" "frontend" {
  name = "frontend"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Frontend"
    push {
      tag = "gcp-[0-9]\\.[0-9]\\.[0-9]"
    }
  }

  substitutions = {
    _REGION          = var.region
    _SERVICE_ACCOUNT = var.front_sa_email
  }

  filename = "cloudbuild.yaml"
}
