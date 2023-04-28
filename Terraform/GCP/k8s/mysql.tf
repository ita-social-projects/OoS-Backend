resource "helm_release" "mysql_operator" {
  name             = "mysql-operator"
  chart            = "../../k8s/infrastructure/charts/mysql-operator-2.0.9.tgz"
  namespace        = "mysql-operator"
  create_namespace = true
  wait             = true
  wait_for_jobs    = true
}

resource "kubernetes_persistent_volume_claim" "backup_pvc" {
  metadata {
    name      = "mysql-backup-pvc"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }
  spec {
    access_modes = ["ReadWriteOnce"]
    resources {
      requests = {
        storage = "10Gi"
      }
    }
    storage_class_name = "standard"
  }
}

resource "helm_release" "mysql" {
  name          = "mysql"
  chart         = "../../k8s/infrastructure/charts/mysql-innodbcluster-2.0.9.tgz"
  namespace     = data.kubernetes_namespace.oos.metadata[0].name
  wait          = true
  wait_for_jobs = true
  values = [
    "${file("${path.module}/values/mysql.yaml")}"
  ]
  set {
    name  = "credentials.root.password"
    value = var.sql_root_pass
  }
  depends_on = [
    helm_release.mysql_operator,
    helm_release.ingress,
    kubernetes_persistent_volume_claim.backup_pvc
  ]
}
