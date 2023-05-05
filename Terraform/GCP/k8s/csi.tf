resource "kubernetes_namespace" "csi" {
  metadata {
    name = "gce-pd-csi-driver"
  }
}

resource "kubernetes_secret" "csi_gcp_credentials" {
  metadata {
    name      = "cloud-sa"
    namespace = kubernetes_namespace.csi.metadata[0].name
  }
  data = {
    "cloud-sa.json" = base64decode(var.csi_sa_key)
  }
}

resource "kubectl_manifest" "admin_binding" {
  yaml_body = <<-EOF
  apiVersion: rbac.authorization.k8s.io/v1
  kind: ClusterRoleBinding
  metadata:
    name: cluster-admin-binding
  roleRef:
    apiGroup: rbac.authorization.k8s.io
    kind: ClusterRole
    name: cluster-admin
  subjects:
    - apiGroup: rbac.authorization.k8s.io
      kind: User
      name: "${var.csi_sa_email}"
  EOF

  ignore_fields = [
    "status",
    "metadata.annotations"
  ]
}

data "kubectl_file_documents" "csi_manifests" {
  content = file("${path.module}/manifests/gcp_csi.yaml")
}

resource "kubectl_manifest" "gcp_cis" {
  count = length(
    flatten(split("\n---\n", file("${path.module}/manifests/gcp_csi.yaml")))
  )
  yaml_body = element(data.kubectl_file_documents.csi_manifests.documents, count.index)
  depends_on = [
    kubernetes_secret.csi_gcp_credentials
  ]

  ignore_fields = [
    "status",
    "metadata.annotations",
  ]
}
