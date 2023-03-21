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
    _REGION          = var.region
    _SERVICE_ACCOUNT = var.front_sa_email
    _SERVICE_NAME    = "frontend"
    _STS_SERVER      = "https://auth.oos.dmytrominochkin.cloud"
    _API_SERVER      = "https://api.oos.dmytrominochkin.cloud"
  }

  filename = "cloudbuild.yaml"
}

resource "google_cloudbuild_trigger" "frontend_stage" {
  name = "frontend-stage"
  github {
    owner = "ita-social-projects"
    name  = "OoS-Frontend"
    push {
      branch = "main"
    }
  }

  substitutions = {
    _REGION          = var.region
    _SERVICE_ACCOUNT = var.front_sa_email
    _SERVICE_NAME    = "frontend-stage"
    _STS_SERVER      = "https://auth.pozashkillia.dmytrominochkin.cloud"
    _API_SERVER      = "https://api.pozashkillia.dmytrominochkin.cloud"
  }

  filename = "cloudbuild.yaml"
}
