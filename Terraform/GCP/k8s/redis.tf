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
