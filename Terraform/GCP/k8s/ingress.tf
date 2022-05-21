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
  depends_on = [
    helm_release.cert-manager,
  ]
}
