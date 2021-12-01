data "google_project" "project" {
}

resource "google_project_service_identity" "build" {
  provider = google-beta

  project = data.google_project.project.project_id
  service = "cloudbuild.googleapis.com"
}

resource "google_service_account_iam_member" "app-acc-user" {
  service_account_id = google_service_account.app.name
  role               = "roles/iam.serviceAccountUser"
  member             = "serviceAccount:${google_project_service_identity.build.email}"
}

resource "google_service_account_iam_member" "auth-acc-user" {
  service_account_id = google_service_account.auth.name
  role               = "roles/iam.serviceAccountUser"
  member             = "serviceAccount:${google_project_service_identity.build.email}"
}

resource "google_service_account_iam_member" "front-acc-user" {
  service_account_id = google_service_account.frontend.name
  role               = "roles/iam.serviceAccountUser"
  member             = "serviceAccount:${google_project_service_identity.build.email}"
}

resource "google_project_iam_member" "run-admin" {
  role   = "roles/run.admin"
  member = "serviceAccount:${google_project_service_identity.build.email}"
}
