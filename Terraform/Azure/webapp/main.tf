resource "azurerm_app_service" "dev" {
  name                = var.name
  location            = var.location
  resource_group_name = var.rg_name
  app_service_plan_id = var.plan

  site_config {
    linux_fx_version          = "DOTNETCORE|3.1"
    scm_type                  = "LocalGit"
    always_on                 = false
    http2_enabled             = true
    use_32_bit_worker_process = local.is_shared ? true : false
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLServer"
    value = var.db_connection
  }

  app_settings = var.app_settings

  tags = var.tags
}

resource "azurerm_app_service_custom_hostname_binding" "dev" {
  hostname            = var.hostname
  app_service_name    = azurerm_app_service.dev.name
  resource_group_name = var.rg_name
  count               = var.hostname != "none" ? 1 : 0
}

//resource "azurerm_app_service_virtual_network_swift_connection" "dev" {
//  app_service_id = azurerm_app_service.dev.id
//  subnet_id      = var.subnet_id
//}
