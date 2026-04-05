# __generated__ by OpenTofu
# Please review these resources and move them into your main configuration files.

# __generated__ by OpenTofu from "08ba9a1c-8caf-435e-bcc4-9c426522c74f"
resource "upstash_redis_database" "cache" {
  auto_scale     = false
  database_name  = "EleveRats-cache"
  eviction       = true
  primary_region = "us-west-2"
  read_regions   = []
  region         = "global"
  tls            = true

  lifecycle {
    prevent_destroy = true
    ignore_changes  = [budget]
  }
}

# __generated__ by OpenTofu
resource "neon_project" "main" {
  allowed_ips_protected_branches_only = null
  block_public_connections            = null
  block_vpc_connections               = null
  compute_provisioner                 = "k8s-neonvm"
  enable_logical_replication          = null
  history_retention_seconds           = 21600
  name                                = "EleveRats"
  org_id                              = "org-shy-base-91444327"
  pg_version                          = 17
  region_id                           = "aws-us-west-2"
  store_password                      = "yes"
  branch {
    database_name = "neondb"
    name          = "production"
    role_name     = "neondb_owner"
  }
  default_endpoint_settings {
    autoscaling_limit_max_cu = 2
    autoscaling_limit_min_cu = 0.25
    suspend_timeout_seconds  = 0
  }
  maintenance_window {
    end_time   = "10:00"
    start_time = "09:00"
    weekdays   = [1]
  }
  quota {
    active_time_seconds  = null
    compute_time_seconds = null
    data_transfer_bytes  = null
    logical_size_bytes   = null
    written_data_bytes   = null
  }
  lifecycle {
    prevent_destroy = true
  }
}

# __generated__ by OpenTofu from "srv-d72o8gp4tr6s738h2l6g"
resource "render_web_service" "api" {
  autoscaling = null
  custom_domains = [
    {
      name = "api-render.gabspassarinhogarcia.uk"
    },
  ]
  disk = null
  env_vars = {
    Cache__Redis__ConnectionString = {
      value = var.render_redis_connection_string
    }
    DATABASE_URL = {
      value = var.render_database_url
    }
    JwtSettings__SecretKey = {
      value = var.render_jwt_secret_key
    }
    OTEL_EXPORTER_OTLP_ENDPOINT = {
      value = var.render_otel_exporter_otlp_endpoint
    }
    OTEL_EXPORTER_OTLP_HEADERS = {
      value = var.render_otel_exporter_otlp_headers
    }
    OTEL_EXPORTER_OTLP_PROTOCOL = {
      value = var.render_otel_exporter_otlp_protocol
    }
    OTEL_RESOURCE_ATTRIBUTES = {
      value = var.render_otel_resource_attributes
    }
    OTEL_SERVICE_NAME = {
      value = var.render_otel_service_name
    }
  }
  environment_id      = "evm-d72ns77fte5s73aear60"
  health_check_path   = "/health"
  ip_allow_list       = null
  log_stream_override = null
  maintenance_mode = {
    enabled = false
    uri     = ""
  }
  max_shutdown_delay_seconds = null
  name                       = "EleveRats"
  notification_override = {
    notifications_to_send         = "default"
    preview_notifications_enabled = "default"
  }
  num_instances      = null
  plan               = "free"
  pre_deploy_command = null
  previews = {
    generation = "automatic"
  }
  region         = "oregon"
  root_directory = ""
  runtime_source = {
    docker = {
      auto_deploy            = false
      auto_deploy_trigger    = "off"
      branch                 = "main"
      build_filter           = null
      context                = "."
      dockerfile_path        = "backend/Dockerfile"
      registry_credential_id = null
      repo_url               = "https://github.com/gabs-passarinho-garcia/EleveRats"
    }
    image          = null
    native_runtime = null
  }
  secret_files  = null
  start_command = null

  lifecycle {
    prevent_destroy = true
    ignore_changes = [
      env_vars,
      runtime_source
    ]
  }
}
