resource "google_monitoring_notification_channel" "email" {
  display_name = "Base Notification Channel"
  type         = "email"
  labels = {
    email_address = var.notification_email
  }
}
