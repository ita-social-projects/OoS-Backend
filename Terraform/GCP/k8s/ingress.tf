resource "helm_release" "ingress" {
  name             = "ingress"
  chart            = "../../k8s/infrastructure/charts/ingress-nginx-4.3.0.tgz"
  namespace        = "ingress-nginx"
  create_namespace = true
  wait             = true
  wait_for_jobs    = true
  values = [
    "${file("${path.module}/values/ingress.yaml")}"
  ]
  set {
    name  = "tcp.${var.sql_port}"
    value = "default/mysql:3306"
  }
  set {
    name  = "tcp.${var.redis_port}"
    value = "default/redis-master:6379"
  }
  set {
    name  = "controller.service.enableHttp"
    value = var.enable_ingress_http
  }
  depends_on = [
    helm_release.cert_manager,
  ]
}
