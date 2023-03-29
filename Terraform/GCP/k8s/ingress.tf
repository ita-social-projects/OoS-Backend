resource "helm_release" "ingress" {
  name             = "ingress"
  chart            = "../../k8s/ingress"
  namespace        = "ingress-nginx"
  create_namespace = true
  wait             = true
  wait_for_jobs    = true
  values = [
    "${file("${path.module}/values/ingress.yaml")}"
  ]
  set {
    name  = "ingress-nginx.tcp.${var.sql_port}"
    value = "default/mysql:3306"
  }
  set {
    name  = "ingress-nginx.tcp.${var.redis_port}"
    value = "default/mysql-redis-master:6379"
  }
  set {
    name  = "ingress-nginx.controller.service.enableHttp"
    value = var.enable_ingress_http
  }
  depends_on = [
    helm_release.cert_manager,
  ]
}
