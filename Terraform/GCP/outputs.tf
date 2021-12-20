output "sql_root_pass" {
  value     = module.passwords.sql_root_pass
  sensitive = true
}

output "sql_api_pass" {
  value     = module.passwords.sql_api_pass
  sensitive = true
}

output "sql_auth_pass" {
  value     = module.passwords.sql_auth_pass
  sensitive = true
}

output "es_admin_pass" {
  value     = module.passwords.es_admin_pass
  sensitive = true
}

output "es_api_pass" {
  value     = module.passwords.es_api_pass
  sensitive = true
}

output "clusterstore_pass" {
  value     = module.sql.db_password
  sensitive = true
}

output "mongo_pass" {
  value     = module.passwords.mongo_pass
  sensitive = true
}

output "mongo_root_pass" {
  value     = module.passwords.mongo_root_pass
  sensitive = true
}
