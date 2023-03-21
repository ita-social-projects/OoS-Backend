data "google_client_openid_userinfo" "me" {}

resource "google_os_login_ssh_public_key" "ssh_key" {
  user = data.google_client_openid_userinfo.me.email
  key  = file(var.ssh_key)
}

resource "google_project_iam_member" "os_login" {
  project = var.project
  role    = "roles/compute.osAdminLogin"
  member  = "user:${var.ssh_user}"
}

resource "google_project_iam_member" "os_login_sa" {
  project = var.project
  role    = "roles/compute.osAdminLogin"
  member  = "serviceAccount:${data.google_client_openid_userinfo.me.email}"
}
