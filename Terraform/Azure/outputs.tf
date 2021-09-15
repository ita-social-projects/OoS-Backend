output "app_azure_git_cmd" {
  value = "git remote add azure_app https://${module.webapp.credentials[0].username}:${module.webapp.credentials[0].password}@${module.webapp.service_name}.scm.azurewebsites.net/${module.webapp.service_name}.git"
}

output "auth_azure_git_cmd" {
  value = "git remote add azure_auth https://${module.auth.credentials[0].username}:${module.auth.credentials[0].password}@${module.auth.service_name}.scm.azurewebsites.net/${module.auth.service_name}.git"
}

output "db_admin" {
  value = module.sql.db_admin
}

output "db_pass" {
  value = module.sql.db_pass
  sensitive = true
}

output "connection_string" {
  value = module.webapp.connection_string
  sensitive = true
}

output "image_blob_connection_string" {
  value = module.storage.image_connection_string
  sensitive = true
}

output "image_blob_container_name" {
  value = module.storage.image_container_name
}

output "app_verification_id" {
  value = module.webapp.verification_id
}

output "auth_verification_id" {
  value = module.auth.verification_id
}