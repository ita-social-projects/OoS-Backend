resource "helm_release" "mongodb" {
  name          = "mongodb"
  chart         = "../../k8s/outofschool"
  namespace     = kubernetes_namespace.oos.metadata[0].name
  wait          = true
  wait_for_jobs = true
  values = [
    "${file("${path.module}/values/mongodb.yaml")}"
  ]
  depends_on = [
    kubernetes_secret.mongodb-credentials,
    helm_release.ingress
  ]
}
