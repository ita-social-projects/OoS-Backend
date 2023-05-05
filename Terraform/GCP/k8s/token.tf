resource "kubernetes_service_account" "deployer" {
  metadata {
    name      = "cloud-build-deploy"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }
}

resource "kubernetes_secret" "deployer" {
  metadata {
    name      = "cloud-build-deploy-token"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
    annotations = {
      "kubernetes.io/service-account.name" = kubernetes_service_account.deployer.metadata[0].name
    }
  }

  type = "kubernetes.io/service-account-token"
}

locals {
  deployer_kubeconfig = <<KUBECONFIG
apiVersion: v1
kind: Config
clusters:
- name: default-cluster
  cluster:
    certificate-authority-data: ${base64encode(kubernetes_secret.deployer.data["ca.crt"])}
    server: https://${var.lb_internal_address}:6443
contexts:
- name: default-context
  context:
    cluster: default-cluster
    namespace: default
    user: default-user
current-context: default-context
users:
- name: default-user
  user:
    token: ${kubernetes_secret.deployer.data["token"]}
KUBECONFIG
}

resource "kubernetes_role" "deployer" {
  metadata {
    name      = "cloud-build-deploy"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }

  rule {
    api_groups = [""]
    resources  = ["pods"]
    verbs      = ["*"]
  }
  rule {
    api_groups = [""]
    resources  = ["secrets"]
    verbs      = ["*"]
  }
  rule {
    api_groups = [""]
    resources  = ["serviceaccounts"]
    verbs      = ["*"]
  }
  rule {
    api_groups = [""]
    resources  = ["services"]
    verbs      = ["*"]
  }
  rule {
    api_groups = ["apps"]
    resources  = ["deployments"]
    verbs      = ["*"]
  }
  rule {
    api_groups = ["apps"]
    resources  = ["replicasets"]
    verbs      = ["*"]
  }
  rule {
    api_groups = ["autoscaling"]
    resources  = ["horizontalpodautoscalers"]
    verbs      = ["*"]
  }
  rule {
    api_groups = ["networking.k8s.io"]
    resources  = ["ingresses"]
    verbs      = ["*"]
  }
}

resource "kubernetes_role_binding" "example" {
  metadata {
    name      = "cloud-build-deploy"
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }
  role_ref {
    api_group = "rbac.authorization.k8s.io"
    kind      = "Role"
    name      = kubernetes_role.deployer.metadata[0].name
  }
  subject {
    kind      = "ServiceAccount"
    name      = kubernetes_service_account.deployer.metadata[0].name
    namespace = data.kubernetes_namespace.oos.metadata[0].name
  }
}
