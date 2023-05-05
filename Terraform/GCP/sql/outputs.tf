output "db_username" {
  value = google_sql_user.default.name
}

output "db_password" {
  value = google_sql_user.default.password
}

output "db_host" {
  value = google_sql_database_instance.storage.private_ip_address
}
