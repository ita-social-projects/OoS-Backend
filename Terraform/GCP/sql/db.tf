data "google_compute_network" "default" {
  name = "default"
}

resource "google_compute_global_address" "private_ip" {
  name          = "sql-private-ip-${var.random_number}"
  purpose       = "VPC_PEERING"
  address_type  = "INTERNAL"
  prefix_length = 16
  network       = data.google_compute_network.default.id
}

resource "google_service_networking_connection" "private_vpc_connection" {
  service                 = "servicenetworking.googleapis.com"
  reserved_peering_ranges = [google_compute_global_address.private_ip.name]
  network                 = data.google_compute_network.default.id
}

resource "google_sql_database_instance" "storage" {
  name                = "cluster-store-${var.random_number}"
  database_version    = "MYSQL_8_0"
  region              = var.region
  deletion_protection = false

  depends_on = [google_service_networking_connection.private_vpc_connection]

  settings {
    tier              = "db-f1-micro"
    availability_type = "ZONAL"
    disk_type         = "PD_HDD"
    disk_autoresize   = true

    ip_configuration {
      ipv4_enabled    = "false"
      private_network = data.google_compute_network.default.id
    }

    backup_configuration {
      enabled    = false
      start_time = "01:00"
    }

    maintenance_window {
      day  = 6
      hour = 1
    }

    location_preference {
      zone = var.zone
    }
  }
}

resource "random_id" "storage_password" {
  keepers = {
    name = google_sql_database_instance.storage.name
  }

  byte_length = 8
  depends_on  = [google_sql_database_instance.storage]
}

resource "google_sql_database" "storage" {
  name       = "k3s"
  instance   = google_sql_database_instance.storage.name
  depends_on = [google_sql_database_instance.storage]
}

resource "google_sql_user" "default" {
  name       = "k3s"
  instance   = google_sql_database_instance.storage.name
  password   = random_id.storage_password.hex
  depends_on = [google_sql_database_instance.storage]
}
