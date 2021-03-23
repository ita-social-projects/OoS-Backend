provider "azurerm" {
  subscription_id = var.subscription_id
  tenant_id       = var.tenant_id
  features {
  }
}

resource "random_integer" "ri" {
  min = 10000
  max = 99999
}

resource "azurerm_resource_group" "dev" {
  name     = var.resource_group_name
  location = var.resource_group_location
  tags     = var.tags
}

module "network" {
  source        = "./network"
  location      = azurerm_resource_group.dev.location
  random_number = random_integer.ri.result
  rg_name       = azurerm_resource_group.dev.name
  tags          = var.tags
}

module "storage" {
  source        = "./storage"
  location      = azurerm_resource_group.dev.location
  random_number = random_integer.ri.result
  rg_name       = azurerm_resource_group.dev.name
  subnet_id     = module.network.subnet_id
  tags          = var.tags
}

module "sql" {
  source        = "./asql"
  location      = azurerm_resource_group.dev.location
  random_number = random_integer.ri.result
  rg_name       = azurerm_resource_group.dev.name
  subnet_id     = module.network.subnet_id
  admin_ip      = var.admin_ip
  tags          = var.tags
}

module "plan" {
  source   = "./plan"
  location = azurerm_resource_group.dev.location
  rg_name  = azurerm_resource_group.dev.name
  tags     = var.tags
  plan = {
    name = "moe-sp-${random_integer.ri.result}"
  }
}

module "auth" {
  source        = "./webapp"
  name          = "moe-auth-${random_integer.ri.result}"
  location      = azurerm_resource_group.dev.location
  rg_name       = azurerm_resource_group.dev.name
  subnet_id     = module.network.subnet_id
  db_connection = local.db_conn_string
  hostname      = var.auth_hostname
  plan          = module.plan.plan_id
  sku_size      = module.plan.sku_size
  tags          = var.tags
  app_settings = {
    "PROJECT" = "OutOfSchool/OutOfSchool.IdentityServer/OutOfSchool.IdentityServer.csproj"
  }
}

module "webapp" {
  source        = "./webapp"
  name          = "moe-app-${random_integer.ri.result}"
  location      = azurerm_resource_group.dev.location
  rg_name       = azurerm_resource_group.dev.name
  subnet_id     = module.network.subnet_id
  db_connection = local.db_conn_string
  hostname      = var.app_hostname
  plan          = module.plan.plan_id
  sku_size      = module.plan.sku_size
  tags          = var.tags
  app_settings = {
    "AzureBlob__ConnectionString"     = module.storage.image_connection_string
    "AzureBlob__StorageContainerName" = module.storage.image_container_name
    "Identity__Authority"             = module.auth.service_url
    "PROJECT"                         = "OutOfSchool/OutOfSchool.WebApi/OutOfSchool.WebApi.csproj"
  }
}

locals {
  db_conn_string = "Server=tcp:${module.sql.db_address},1433;Initial Catalog=${module.sql.db_name};Persist Security Info=False;User ID=${module.sql.db_admin};Password=${module.sql.db_pass};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
