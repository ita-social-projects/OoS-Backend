# terraform {
#   required_providers {
#     kubernetes = {
#       source = "hashicorp/kubernetes"
#     }
#   }
# }

locals {
  https_port = element([for port in data.kubernetes_service.nginx.spec[0].port : port if port.name == "https"], 0)
}

data "kubernetes_service" "nginx" {
  metadata {
    name      = "${var.ingress_name}-ingress-nginx-controller"
    namespace = "ingress-nginx"
  }
}

# module "gce_lb_https" {
#   source            = "GoogleCloudPlatform/lb-http/google"
#   version           = "~> 6.2.0"
#   name              = "iap-https-${var.random_number}"
#   project           = var.project
#   ssl               = false
#   firewall_networks = [var.network_name]

#   target_tags = var.tags

#   url_map        = google_compute_url_map.iap_url_map.self_link
#   create_url_map = false

#   backends = {
#     default = {
#       description                     = null
#       protocol                        = "HTTPS"
#       port                            = data.kubernetes_service.nginx.spec[0].port[0].node_port
#       port_name                       = data.kubernetes_service.nginx.spec[0].port[0].name
#       timeout_sec                     = 10
#       connection_draining_timeout_sec = null
#       enable_cdn                      = false
#       security_policy                 = null
#       session_affinity                = null
#       affinity_cookie_ttl_sec         = null
#       custom_request_headers          = null
#       custom_response_headers         = null

#       health_check = {
#         check_interval_sec  = null
#         timeout_sec         = null
#         healthy_threshold   = null
#         unhealthy_threshold = null
#         request_path        = "/"
#         port                = data.kubernetes_service.nginx.spec[0].port[0].node_port
#         host                = null
#         logging             = false
#       }

#       log_config = {
#         enable      = false
#         sample_rate = 0.5
#       }

#       groups = [
#         {
#           group                        = var.mig_url
#           balancing_mode               = null
#           capacity_scaler              = null
#           description                  = null
#           max_connections              = null
#           max_connections_per_instance = null
#           max_connections_per_endpoint = null
#           max_rate                     = null
#           max_rate_per_instance        = null
#           max_rate_per_endpoint        = null
#           max_utilization              = null
#         },
#       ]

#       iap_config = {
#         enable               = false
#         oauth2_client_id     = ""
#         oauth2_client_secret = ""
#       }
#     }
#   }

# }

# resource "google_compute_url_map" "iap_url_map" {
#   name            = "iap-https-${var.random_number}"
#   default_service = module.gce_lb_https.backend_services["default"].self_link

#   host_rule {
#     hosts        = ["*"]
#     path_matcher = "allpaths"
#   }

#   path_matcher {
#     name            = "allpaths"
#     default_service = module.gce_lb_https.backend_services["default"].self_link
#   }
# }
