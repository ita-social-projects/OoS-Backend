resource "kubernetes_secret" "sql_credentials" {
  metadata {
    name      = "mysql-auth"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    mysql-root-password        = var.sql_root_pass
    mysql-replication-password = var.sql_auth_pass
    mysql-password             = var.sql_auth_pass
  }
}

resource "kubernetes_secret" "sql_api_credentials" {
  metadata {
    name      = "mysql-api-auth"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  data = {
    API_PASSWORD = var.sql_api_pass
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