output "image_connection_string" {
  value = azurerm_storage_account.images.primary_connection_string
}

output "image_container_name" {
  value = azurerm_storage_container.images.name
}
