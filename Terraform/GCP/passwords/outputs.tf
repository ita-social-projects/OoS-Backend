output "sql_root_pass" {
  value = random_password.sql_root_pass.result
}

output "sql_api_pass" {
  value = random_password.sql_api_pass.result
}

output "sql_auth_pass" {
  value = random_password.sql_auth_pass.result
}

output "es_admin_pass" {
  value = random_password.es_admin_pass.result
}

output "es_api_pass" {
  value = random_password.es_api_pass.result
}
