resource "google_monitoring_alert_policy" "iam_role" {
  display_name = "IAM Role Change Policy"
  combiner     = "OR"
  conditions {
    display_name = "custom role operation"
    condition_threshold {
      filter          = "metric.type=\"logging.googleapis.com/user/${google_logging_metric.iam_role.id}\" AND resource.type=\"global\""
      duration        = "60s"
      comparison      = "COMPARISON_GT"
      threshold_value = 0
    }
  }

  notification_channels = [
    google_monitoring_notification_channel.email.name
  ]
}

# resource "google_monitoring_alert_policy" "bucket_permissions" {
#   display_name = "Bucket Permissions Policy"
#   combiner     = "OR"
#   conditions {
#     display_name = "custom bucket operation"
#     condition_threshold {
#       filter          = "metric.type=\"logging.googleapis.com/user/${google_logging_metric.bucket_permissions.id}\" AND resource.type=\"gcs_bucket\""
#       duration        = "60s"
#       comparison      = "COMPARISON_GT"
#       threshold_value = 0
#       aggregations {
#         per_series_aligner = "ALIGN_NONE"
#       }
#     }
#   }

#   notification_channels = [
#     google_monitoring_notification_channel.email.name
#   ]
# }
