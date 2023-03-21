locals {
  services = var.cloud_run_lb.enable ? var.cloud_run_lb.services : {}
}

resource "google_compute_global_address" "lb" {
  name = "run-static-${var.random_number}"
}

# TODO: this works if services were already deployed by CD in the build module :(
data "google_cloud_run_service" "services" {
  for_each = local.services
  name     = each.key
  location = var.region
}

resource "google_compute_region_network_endpoint_group" "serverless_neg" {
  for_each              = local.services
  provider              = google-beta
  name                  = each.key
  network_endpoint_type = "SERVERLESS"
  region                = var.region
  project               = var.project
  cloud_run {
    service = data.google_cloud_run_service.services[each.key].name
  }

  lifecycle {
    create_before_destroy = true
  }
}

resource "google_compute_ssl_policy" "custom_ssl_policy" {
  name            = "ssl-policy-${var.random_number}"
  profile         = "RESTRICTED"
  min_tls_version = "TLS_1_2"
}

module "cr_lb_http" {
  source  = "GoogleCloudPlatform/lb-http/google//modules/serverless_negs"
  version = "~> 6.2.0"
  name    = "cr-lb-${var.random_number}"
  project = var.project

  ssl                             = var.cloud_run_lb.ssl
  ssl_policy                      = var.cloud_run_lb.ssl ? google_compute_ssl_policy.custom_ssl_policy.self_link : null
  managed_ssl_certificate_domains = [for k, v in local.services : v.domain]
  https_redirect                  = var.cloud_run_lb.ssl
  create_url_map                  = false
  url_map                         = google_compute_url_map.cr_url_map.self_link
  http_forward                    = !var.cloud_run_lb.ssl
  create_address                  = false
  address                         = google_compute_global_address.lb.address
  quic                            = true


  backends = { for service in google_compute_region_network_endpoint_group.serverless_neg :
    service.name => {
      description = null
      groups = [
        {
          group = service.id
        }
      ]
      enable_cdn              = false
      security_policy         = null
      custom_request_headers  = null
      custom_response_headers = null

      iap_config = {
        enable               = false
        oauth2_client_id     = ""
        oauth2_client_secret = ""
      }
      log_config = {
        enable      = false
        sample_rate = null
      }
    }
  }
}

resource "google_compute_url_map" "cr_url_map" {
  name            = "cr-map-${var.random_number}"
  default_service = module.cr_lb_http.backend_services["frontend"].self_link

  dynamic "host_rule" {
    for_each = local.services
    content {
      hosts        = [host_rule.value["domain"]]
      path_matcher = host_rule.key
    }
  }

  dynamic "path_matcher" {
    for_each = local.services
    content {
      name            = path_matcher.key
      default_service = module.cr_lb_http.backend_services[path_matcher.key].self_link
    }
  }
}
