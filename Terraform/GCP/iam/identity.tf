resource "google_service_account" "auth" {
  account_id   = "auth-run-${var.random_number}"
  display_name = "Identity Service Account"
}

resource "google_project_iam_member" "secret-accessor-auth" {
  role   = "roles/secretmanager.secretAccessor"
  member = "serviceAccount:${google_service_account.auth.email}"
}