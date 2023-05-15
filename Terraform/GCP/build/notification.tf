data "archive_file" "app" {
  type = "zip"

  source {
    content  = file("${path.module}/notification/main.py")
    filename = "main.py"
  }
  source {
    content  = file("${path.module}/notification/requirements.txt")
    filename = "requirements.txt"
  }
  output_path = "${path.module}/index.zip"
}

resource "google_storage_bucket_object" "archive" {
  name   = "index.zip"
  bucket = var.gcf_bucket
  source = "${path.module}/index.zip"
}

resource "google_cloudfunctions2_function" "function" {
  name        = "discord-notification-${var.random_number}"
  location    = var.region
  description = "Discord build notification"

  build_config {
    runtime     = "python311"
    entry_point = "subscribe"
    source {
      storage_source {
        bucket = var.gcf_bucket
        object = google_storage_bucket_object.archive.name
      }
    }
  }

  service_config {
    max_instance_count             = 3
    available_memory               = "128Mi"
    timeout_seconds                = 60
    ingress_settings               = "ALLOW_INTERNAL_ONLY"
    all_traffic_on_latest_revision = true
    service_account_email          = var.gcf_sa_email
    environment_variables = {
      WEBHOOK_URL = var.discord_notification_webhook
    }
  }

  event_trigger {
    trigger_region = var.region
    event_type     = "google.cloud.pubsub.topic.v1.messagePublished"
    pubsub_topic   = google_pubsub_topic.cloud_build.id
    retry_policy   = "RETRY_POLICY_DO_NOT_RETRY"
    # retry_policy   = "RETRY_POLICY_RETRY"
  }
}
