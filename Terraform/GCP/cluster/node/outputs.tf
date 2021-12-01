output "mig_url" {
  value = google_compute_instance_group_manager.k3s.instance_group
}

output "template_id" {
  value = google_compute_instance_template.k3s.id
}
