output "es_api_secret" {
  value = "${element(local.es_api_list, length(local.es_api_list) - 3)}:${element(local.es_api_list, length(local.es_api_list) - 1)}"
}

output "sql_api_secret" {
  value = "${element(local.api_list, length(local.api_list) - 3)}:${element(local.api_list, length(local.api_list) - 1)}"
}

output "sql_auth_secret" {
  value = "${element(local.auth_list, length(local.auth_list) - 3)}:${element(local.auth_list, length(local.auth_list) - 1)}"
}

output "redis_secret" {
  value = "${element(local.redis_list, length(local.redis_list) - 3)}:${element(local.redis_list, length(local.redis_list) - 1)}"
}

output "sendgrid_key_secret" {
  value = "${element(local.sendgrid_key_list, length(local.sendgrid_key_list) - 3)}:${element(local.sendgrid_key_list, length(local.sendgrid_key_list) - 1)}"
}

output "github_secret" {
  value = google_secret_manager_secret_version.github_secret.name
}

output "github_token_secret" {
  value = google_secret_manager_secret_version.github_token_secret.name
}
