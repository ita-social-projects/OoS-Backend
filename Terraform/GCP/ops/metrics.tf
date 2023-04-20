resource "google_logging_metric" "iam_role" {
  project = var.project
  name    = "iam_role_change"
  filter  = "resource.type=\"iam_role\" AND protoPayload.methodName = \"google.iam.admin.v1.CreateRole\" OR protoPayload.methodName=\"google.iam.admin.v1.DeleteRole\" OR protoPayload.methodName=\"google.iam.admin.v1.UpdateRole\""
  metric_descriptor {
    metric_kind = "DELTA"
    value_type  = "INT64"
    labels {
      key        = "operation"
      value_type = "STRING"
    }
    labels {
      key        = "role"
      value_type = "STRING"
    }

  }
  label_extractors = {
    "operation" = "EXTRACT(protoPayload.methodName)"
    "role"      = "EXTRACT(protoPayload.resourceName)"
  }
}

# resource "google_logging_metric" "bucket_permissions" {
#   project = var.project
#   name    = "bucket_permissions"
#   filter  = "resource.type=\"gcs_bucket\" AND protoPayload.methodName=\"storage.setIamPermissions\""
#   metric_descriptor {
#     metric_kind = "DELTA"
#     value_type  = "INT64"
#     labels {
#       key        = "bucket"
#       value_type = "STRING"
#     }
#   }
#   label_extractors = {
#     "bucket" = "EXTRACT(protoPayload.resourceName)"
#   }
# }

# resource "google_logging_metric" "audit_config" {
#   project = var.project
#   name    = "audit_config"
#   filter  = "protoPayload.methodName=\"SetIamPolicy\" AND protoPayload.serviceData.policyDelta.auditConfigDeltas:*"
#   metric_descriptor {
#     metric_kind = "DELTA"
#     value_type  = "INT64"
#     labels {
#       key        = "audit"
#       value_type = "STRING"
#     }
#   }
#   label_extractors = {
#     "audit" = "EXTRACT(protoPayload.resourceName)"
#   }
# }
