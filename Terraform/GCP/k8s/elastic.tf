resource "kubectl_manifest" "elastic-ssl" {
  yaml_body = <<-EOF
  apiVersion: cert-manager.io/v1
  kind: Certificate
  metadata:
    name: elastic-certificates
    namespace: ${kubernetes_namespace.oos.metadata[0].name}
  spec:
    dnsNames:
      - elasticsearch-master
      - elasticsearch-master.outofschool.svc
      - elasticsearch-master.outofschool.svc.cluster.local
    duration: 2160h0m0s
    issuerRef:
      kind: Issuer
      name: ${kubectl_manifest.oos-issuer.name}
    renewBefore: 168h0m0s
    secretName: elastic-certificates
  EOF
}

resource "helm_release" "elastic" {
  name          = "elastic"
  chart         = "../../k8s/outofschool"
  namespace     = kubernetes_namespace.oos.metadata[0].name
  wait          = true
  wait_for_jobs = true
  timeout       = 600
  values = [
    "${file("${path.module}/values/elastic.yaml")}"
  ]
  set {
    name  = "kibana.ingress.annotations.nginx\\.ingress\\.kubernetes\\.io/whitelist-source-range"
    value = join("\\,", var.admin_ips)
  }
  set {
    name  = "elastic.ingress.tls[0].hosts[0]"
    value = var.elastic_hostname
  }
  set {
    name  = "elastic.ingress.hosts[0].host"
    value = var.elastic_hostname
  }
  set {
    name  = "kibana.ingress.tls[0].hosts[0]"
    value = var.kibana_hostname
  }
  set {
    name  = "kibana.ingress.hosts[0].host"
    value = var.kibana_hostname
  }
  depends_on = [
    kubernetes_secret.elastic-credentials,
    kubectl_manifest.elastic-ssl,
    helm_release.ingress
  ]
}
