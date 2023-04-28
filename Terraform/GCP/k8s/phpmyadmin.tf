resource "helm_release" "phpmyadmin" {
  name          = "phpmyadmin"
  chart         = "../../k8s/infrastructure/charts/phpmyadmin-11.0.0.tgz"
  namespace     = data.kubernetes_namespace.oos.metadata[0].name
  wait          = true
  wait_for_jobs = true
  values = [
    "${file("${path.module}/values/phpmyadmin.yaml")}"
  ]
  set {
    name  = "ingress.annotations.nginx\\.ingress\\.kubernetes\\.io/whitelist-source-range"
    value = join("\\,", var.admin_ips)
  }
  set {
    name  = "ingress.hostname"
    value = var.phpmyadmin_hostname
  }
  depends_on = [
    helm_release.ingress,
    helm_release.mysql
  ]
}
