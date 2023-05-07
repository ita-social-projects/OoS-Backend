output "webapi_sa_email" {
  value = google_service_account.app.email
}

output "identity_sa_email" {
  value = google_service_account.auth.email
}

output "frontend_sa_email" {
  value = google_service_account.frontend.email
}

output "gke_sa_email" {
  value = google_service_account.gke.email
}

output "csi_sa_email" {
  value = google_service_account.csi.email
}

output "csi_sa_key" {
  value = google_service_account_key.csi.private_key
}

output "pull_sa_email" {
  value = google_service_account.csi.email
}

output "pull_sa_key" {
  value = google_service_account_key.pull.private_key
}

output "gcf_sa_email" {
  value = google_service_account.notification.email
}
