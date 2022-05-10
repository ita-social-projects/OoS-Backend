data "google_compute_image" "ubuntu" {
  name = "ubuntu-2004-focal-v20220419"
  # family = "ubuntu-2004-lts"
  project = "ubuntu-os-cloud"
}
