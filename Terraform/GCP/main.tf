provider "google" {
  project     = var.project
  region      = var.region
  zone        = var.zone
  credentials = file(var.credentials)
}

resource "random_integer" "ri" {
  min = 10000
  max = 99999
}

module "storage" {
  source        = "./storage"
  random_number = random_integer.ri.result
  region        = var.region
}

module "iam" {
  source             = "./iam"
  random_number      = random_integer.ri.result
  access_group_email = var.access_group_email
  project            = var.project
  bucket             = module.storage.image-bucket
}

module "passwords" {
  source = "./passwords"
}

module "sql" {
  source        = "./sql"
  zone          = var.zone
  region        = var.region
  random_number = random_integer.ri.result
}

module "cluster" {
  source           = "./cluster"
  project          = var.project
  zone             = var.zone
  region           = var.region
  labels           = var.labels
  random_number    = random_integer.ri.result
  sa_email         = module.iam.gke_sa_email
  admin_ips        = var.admin_ips
  k8s_api_hostname = var.k8s_api_hostname
  credentials      = var.credentials
  ssh_user         = var.ssh_user
  ssh_key          = var.ssh_key
  db_username      = module.sql.db_username
  db_password      = module.sql.db_password
  db_host          = module.sql.db_host
}

resource "time_sleep" "wait_30_seconds" {
  depends_on = [module.cluster]

  destroy_duration = "30s"
}

provider "kubernetes" {
  config_path = "./cluster/kubeconfig.yaml"
}

provider "helm" {
  kubernetes {
    config_path = "./cluster/kubeconfig.yaml"
  }
}

provider "kubectl" {
  config_path = "./cluster/kubeconfig.yaml"
}

module "k8s" {
  source              = "./k8s"
  project             = var.project
  zone                = var.zone
  admin_ips           = var.admin_ips
  sql_root_pass       = module.passwords.sql_root_pass
  sql_api_pass        = module.passwords.sql_api_pass
  sql_auth_pass       = module.passwords.sql_auth_pass
  es_admin_pass       = module.passwords.es_admin_pass
  es_api_pass         = module.passwords.es_api_pass
  redis_pass          = module.passwords.redis_pass
  csi_sa_email        = module.iam.csi_sa_email
  csi_sa_key          = module.iam.csi_sa_key
  letsencrypt_email   = var.letsencrypt_email
  sql_hostname        = var.sql_hostname
  phpmyadmin_hostname = var.phpmyadmin_hostname
  kibana_hostname     = var.kibana_hostname
  elastic_hostname    = var.elastic_hostname
  depends_on = [
    time_sleep.wait_30_seconds
  ]
}

module "secrets" {
  source        = "./secrets"
  sql_api_pass  = module.passwords.sql_api_pass
  sql_auth_pass = module.passwords.sql_auth_pass
  es_api_pass   = module.passwords.es_api_pass
  redis_pass    = module.passwords.redis_pass
  labels        = var.labels
  sql_hostname  = var.sql_hostname
  sendgrid_key  = var.sendgrid_key
}

module "build" {
  source              = "./build"
  app_sa_email        = module.iam.webapi_sa_email
  auth_sa_email       = module.iam.identity_sa_email
  front_sa_email      = module.iam.frontend_sa_email
  project             = var.project
  zone                = var.zone
  region              = var.region
  api_secret          = module.secrets.sql_api_secret
  auth_secret         = module.secrets.sql_auth_secret
  es_api_pass_secret  = module.secrets.es_api_secret
  redis_hostname      = var.redis_hostname
  redis_secret        = module.secrets.redis_secret
  sender_email        = var.sender_email
  sendgrid_key_secret = module.secrets.sendgrid_key_secret
  bucket              = module.storage.image-bucket
}
