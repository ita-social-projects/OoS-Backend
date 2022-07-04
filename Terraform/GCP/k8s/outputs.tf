output "ingress_name" {
  value = helm_release.ingress.metadata[0].name
}
