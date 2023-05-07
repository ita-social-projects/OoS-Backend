resource "google_service_account" "notification" {
  account_id   = "discord-notification-${var.random_number}"
  display_name = "Service account for Discord GCF"
}

resource "google_project_iam_member" "pubsub" {
  role    = "roles/iam.serviceAccountTokenCreator"
  member  = "serviceAccount:service-${data.google_project.project.number}@gcp-sa-pubsub.iam.gserviceaccount.com"
  project = var.project
}

resource "google_project_iam_member" "eventarc" {
  role    = "roles/eventarc.eventReceiver"
  member  = "serviceAccount:${google_service_account.notification.email}"
  project = var.project
}
