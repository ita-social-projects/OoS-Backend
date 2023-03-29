resource "helm_release" "vector_logs" {
  name          = var.vector_logs_name
  chart         = var.vector_logs_chart_path
  namespace     = var.vector_logs_namespace
  wait          = true
  wait_for_jobs = true
  timeout       = 600
  values = [
    "${file("${path.module}/values/vector.yaml")}"
  ]
  set {
    name  = "secrets.generic.es_user"
    value = var.es_admin_user
  }
  set {
    name  = "secrets.generic.es_password"
    value = var.es_admin_pass
  }
  set {
    name  = "secrets.generic.es_endpoint"
    value = var.elastic_url
  }

  depends_on = [
    kubernetes_secret.elastic_credentials,
    kubectl_manifest.elastic_ssl,
    helm_release.ingress
  ]
}