data "google_compute_image" "ubuntu" {
  name = "ubuntu-2204-jammy-v20230429"
  # family = "ubuntu-2204-lts"
  project = "ubuntu-os-cloud"
}
