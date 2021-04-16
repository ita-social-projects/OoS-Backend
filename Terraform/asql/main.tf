resource "random_password" "db_pass" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "azurerm_sql_server" "db" {
  name                         = "moe-sql-${var.random_number}"
  resource_group_name          = var.rg_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "odmin-${var.random_number}"
  administrator_login_password = random_password.db_pass.result

  tags = var.tags
}

resource "azurerm_mssql_database" "db" {
  name        = "moe-db-${var.random_number}"
  server_id   = azurerm_sql_server.db.id
  create_mode = "Default"

  auto_pause_delay_in_minutes = 60
  min_capacity                = 0.5
  max_size_gb                 = 2
  sku_name                    = "Basic"

  tags = var.tags
}

// Allow access to Azure Services
resource "azurerm_sql_firewall_rule" "azure" {
  name                = "allow-azure-services-${var.random_number}"
  resource_group_name = var.rg_name
  server_name         = azurerm_sql_server.db.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}

resource "azurerm_sql_virtual_network_rule" "subnet" {
  name                = "allow-subnet-${var.random_number}"
  resource_group_name = var.rg_name
  server_name         = azurerm_sql_server.db.name
  subnet_id           = var.subnet_id
}

// Allow access to Admin IP
resource "azurerm_sql_firewall_rule" "admin" {
  name                = "allow-admin-ip-${var.random_number}"
  resource_group_name = var.rg_name
  server_name         = azurerm_sql_server.db.name
  start_ip_address    = var.admin_ip
  end_ip_address      = var.admin_ip
  count               = var.admin_ip == "none" ? 0 : 1
}
