output "db_pass" {
  value = azurerm_sql_server.db.administrator_login_password
}

output "db_admin" {
  value = azurerm_sql_server.db.administrator_login
}

output "db_name" {
  value = azurerm_mssql_database.db.name
}

output "db_address" {
  value = azurerm_sql_server.db.fully_qualified_domain_name
}
