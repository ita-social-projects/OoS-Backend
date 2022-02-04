remote_state {
    backend = "gcs"
    generate = {
        path      = "backend.tf"
        if_exists = "overwrite"
    }
    config = {
        bucket       = get_env("TERRAGRUNT_BUCKET", "moetfstate")
        project      = get_env("TERRAGRUNT_PROJECT")
        credentials  = get_env("GOOGLE_APPLICATION_CREDENTIALS")
        location     = "europe-west1"
        prefix       = "${path_relative_to_include()}/terraform.tfstate"
    }
}

terraform {
    extra_arguments "core_vars" {
        commands = get_terraform_commands_that_need_vars()

        arguments = [
            "-var-file=./gcp.tfvars"
        ]
    }
}