output "service_name" {
  value = azurerm_app_service.dev.name
}

output "credentials" {
  value = azurerm_app_service.dev.site_credential
}

output "connection_string" {
  value = azurerm_app_service.dev.connection_string
}

output "verification_id" {
  value = azurerm_app_service.dev.custom_domain_verification_id
}

output "service_url" {
  value = var.hostname != "none" ? "http://${var.hostname}" : "https://${azurerm_app_service.dev.default_site_hostname}"
}
