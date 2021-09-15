resource "azurerm_virtual_network" "dev" {
  name                = "moe-vnet-${var.random_number}"
  address_space       = ["10.1.0.0/16"]
  location            = var.location
  resource_group_name = var.rg_name

  tags = var.tags
}

resource "azurerm_subnet" "dev" {
  name                 = "default-${var.random_number}"
  resource_group_name  = var.rg_name
  virtual_network_name = azurerm_virtual_network.dev.name
  address_prefixes     = ["10.1.0.0/24"]
  service_endpoints    = ["Microsoft.Sql", "Microsoft.Storage", "Microsoft.Web"]

  delegation {
    name = "acctestdelegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

