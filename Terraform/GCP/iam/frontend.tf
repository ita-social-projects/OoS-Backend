resource "google_service_account" "frontend" {
  account_id   = "front-run-${var.random_number}"
  display_name = "Frontend Service Account"
}