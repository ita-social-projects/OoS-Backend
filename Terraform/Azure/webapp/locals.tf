locals {
  is_shared = contains(["F1", "FREE", "D1", "SHARED"], upper(var.sku_size))
}
