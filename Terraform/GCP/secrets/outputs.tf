output "es_api_secret" {
  value = "${element(local.es_api_list, length(local.es_api_list) - 3)}:${element(local.es_api_list, length(local.es_api_list) - 1)}"
}

output "sql_api_secret" {
  value = "${element(local.api_list, length(local.api_list) - 3)}:${element(local.api_list, length(local.api_list) - 1)}"
}

output "sql_auth_secret" {
  value = "${element(local.auth_list, length(local.auth_list) - 3)}:${element(local.auth_list, length(local.auth_list) - 1)}"
}
