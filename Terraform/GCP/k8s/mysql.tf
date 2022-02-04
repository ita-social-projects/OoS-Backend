resource "helm_release" "mysql" {
  name          = "mysql"
  chart         = "../../k8s/outofschool"
  namespace     = kubernetes_namespace.oos.metadata[0].name
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
    kubernetes_secret.sql-credentials,
    kubernetes_secret.sql-api-credentials,
    helm_release.ingress
  ]
}
