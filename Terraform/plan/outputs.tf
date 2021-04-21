output "plan_id" {
  value = azurerm_app_service_plan.dev.id
}

output "sku_size" {
  value = local.plan.sku_size
}
