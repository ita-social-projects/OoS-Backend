resource "kubernetes_secret" "sql_api_credentials" {
  metadata {
    name      = "mysql-api-auth"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    API_PASSWORD      = var.sql_api_pass
    IDENTITY_PASSWORD = var.sql_auth_pass
  }
}

resource "kubernetes_secret" "elastic_credentials" {
  metadata {
    name      = "elasticsearch-credentials"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    username = "elastic"
    password = var.es_admin_pass
    apipass  = var.es_api_pass
  }
}

resource "kubernetes_secret" "redis_credentials" {
  metadata {
    name      = "redis-auth"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    password = var.redis_pass
  }
}

resource "kubernetes_secret" "pull" {
  metadata {
    name = "outofschool-gcp-pull-secrets"
  }

  type = "kubernetes.io/dockerconfigjson"

  data = {
    ".dockerconfigjson" = jsonencode({
      auths = {
        "https://gcr.io" = {
          "username" = "_json_key"
          "password" = trimspace(base64decode(var.pull_sa_key))
          "email"    = var.pull_sa_email
          "auth"     = base64encode(join(":", ["_json_key", base64decode(var.pull_sa_key)]))
        }
      }
    })
  }
}

# TODO: Do we really need this enabled?:)
resource "random_password" "api_secret" {
  length           = 16
  special          = true
  override_special = "_%@"
}

# TODO: Do we really need this enabled?:)
resource "random_password" "client_secret" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "kubernetes_secret" "authserver_secrets" {
  metadata {
    name      = "authserver-secrets"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    Email__SendGridKey         = var.sendgrid_key
    outofschoolapi__ApiSecret  = random_password.api_secret.result
    "m2m.client__ClientSecret" = random_password.client_secret.result
  }
}

resource "kubernetes_secret" "webapi_secrets" {
  metadata {
    name      = "webapi-secrets"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    GeoCoding__ApiKey = var.geo_apikey
  }
}
