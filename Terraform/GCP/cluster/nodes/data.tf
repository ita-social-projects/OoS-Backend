data "google_compute_image" "ubuntu" {
  name = "ubuntu-2004-focal-v20221213"
  # family = "ubuntu-2004-lts"
  project = "ubuntu-os-cloud"
}
