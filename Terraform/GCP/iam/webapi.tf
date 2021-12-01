resource "google_service_account" "app" {
  account_id   = "app-run-${var.random_number}"
  display_name = "Application Service Account"
}

resource "google_project_iam_member" "secret-accessor-api" {
  role   = "roles/secretmanager.secretAccessor"
  member = "serviceAccount:${google_service_account.app.email}"
}