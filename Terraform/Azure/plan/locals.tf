locals {
  plan = merge({
    name     = ""
    sku_size = "B1"
    os_type  = "Linux",
    tier     = "Basic"
  }, var.plan)
}
