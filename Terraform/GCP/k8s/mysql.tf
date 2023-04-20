resource "helm_release" "mysql" {
  name          = "mysql"
  chart         = "../../k8s/database"
  namespace     = data.kubernetes_namespace.oos.metadata[0].name
  wait          = true
  wait_for_jobs = true
  values = [
    "${file("${path.module}/values/mysql.yaml")}"
  ]
  set {
    name  = "phpmyadmin.ingress.annotations.nginx\\.ingress\\.kubernetes\\.io/whitelist-source-range"
    value = join("\\,", var.admin_ips)
  }
  set {
    name  = "phpmyadmin.ingress.hostname"
    value = var.phpmyadmin_hostname
  }
  depends_on = [
    kubernetes_secret.sql_credentials,
    kubernetes_secret.sql_api_credentials,
    helm_release.ingress
  ]
}

resource "helm_release" "redis" {
  name          = "redis"
  chart         = "../../k8s/infrastructure/charts/redis-17.9.3.tgz"
  namespace     = data.kubernetes_namespace.oos.metadata[0].name
  wait          = true
  wait_for_jobs = true
  values = [
    "${file("${path.module}/../../../k8s/infrastructure/redis.yaml")}"
  ]
  depends_on = [
    kubernetes_secret.redis_credentials,
    helm_release.ingress
  ]
}
