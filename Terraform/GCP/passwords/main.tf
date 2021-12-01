resource "random_password" "sql_root_pass" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "sql_api_pass" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "sql_auth_pass" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "es_admin_pass" {
  length           = 16
  special          = true
  override_special = "_%@"
}

resource "random_password" "es_api_pass" {
  length           = 16
  special          = true
  override_special = "_%@"
}
