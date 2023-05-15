resource "google_cloudbuild_trigger" "backend_auth_old" {
  name     = "backend-auth-old"
  disabled = true
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

  filename = "cloudbuild-auth-old.yml"
}

resource "google_cloudbuild_trigger" "frontend_old" {
  name     = "frontend-old"
  disabled = true
  github {
    owner = "ita-social-projects"
    name  = "OoS-Frontend"
    push {
      branch = "develop"
    }
  }

  substitutions = {
    _REGION          = var.region
    _SERVICE_ACCOUNT = var.front_sa_email
    _SERVICE_NAME    = "frontend"
    _STS_SERVER      = "https://${var.auth_hostname}"
    _API_SERVER      = "https://${var.app_hostname}"
    _GITHUB_DEPLOY   = var.github_front_secret
  }

  filename = "cloudbuild-old.yaml"
}
