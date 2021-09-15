resource "azurerm_storage_account" "images" {
  name                = "moeimage${var.random_number}"
  resource_group_name = var.rg_name

  location                 = var.location
  account_tier             = "Standard"
  account_kind             = "StorageV2"
  account_replication_type = "LRS"
  allow_blob_public_access = true

  //  network_rules {
  //    default_action             = "Deny"
  ////    ip_rules                   = ["100.0.0.1"]
  //    virtual_network_subnet_ids = [var.subnet_id]
  //  }

  tags = var.tags
}

resource "azurerm_storage_container" "images" {
  name                  = "moe-image-${var.random_number}"
  storage_account_name  = azurerm_storage_account.images.name
  container_access_type = "blob"
}
