output "mig_url" {
  value = module.masters.mig_url
}

output "lb_internal_address" {
  value = google_compute_address.lb_internal.address
}
