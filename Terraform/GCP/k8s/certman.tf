resource "helm_release" "cert_manager" {
  name             = "cert-manager"
  chart            = "../../k8s/cert-manager"
  namespace        = "cert-manager"
  create_namespace = true
  wait             = true
  wait_for_jobs    = true
}
