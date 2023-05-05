output "mig_url" {
  value = var.node_role == "master" ? google_compute_instance_group_manager.k3s[0].instance_group : google_compute_instance_group_manager.k3s_worker[0].instance_group
}

output "template_id" {
  value = google_compute_instance_template.k3s.id
}

output "names" {
  value = local.full_names
}
