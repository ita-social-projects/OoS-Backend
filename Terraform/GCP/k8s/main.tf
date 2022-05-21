terraform {
  required_providers {
    kubernetes = {
      source = "hashicorp/kubernetes"
    }
    helm = {
      source = "hashicorp/helm"
    }
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = ">= 1.7.0"
    }
  }
}

data "kubernetes_namespace" "oos" {
  metadata {
    name = "default"
  }
}

resource "kubectl_manifest" "sc" {
  yaml_body = <<-EOF
  allowVolumeExpansion: true
  apiVersion: storage.k8s.io/v1
  kind: StorageClass
  metadata:
    annotations:
      storageclass.kubernetes.io/is-default-class: "true"
    labels:
      addonmanager.kubernetes.io/mode: EnsureExists
    name: standard
  parameters:
    type: pd-standard
  provisioner: pd.csi.storage.gke.io
  volumeBindingMode: Immediate
  EOF
}
