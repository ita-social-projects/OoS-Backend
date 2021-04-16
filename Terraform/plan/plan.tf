resource "azurerm_app_service_plan" "dev" {
  name                = local.plan.name
  location            = var.location
  resource_group_name = var.rg_name
  kind                = local.plan.os_type
  reserved            = local.plan.os_type == "Linux" ? true : null

  sku {
    tier = local.plan.tier
    size = local.plan.sku_size
  }

  tags = var.tags

  lifecycle {
    create_before_destroy = true
  }
}
