data "google_compute_image" "ubuntu" {
  name = "ubuntu-2004-focal-v20220118"
  # family = "ubuntu-2004-lts"
  project = "ubuntu-os-cloud"
}
