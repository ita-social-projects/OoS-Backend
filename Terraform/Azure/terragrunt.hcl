remote_state {
    backend = "azurerm"
    generate = {
        path      = "backend.tf"
        if_exists = "overwrite"
    }
    config = {
        resource_group_name  = get_env("TERRAGRUNT_RESOURCE_GROUP", "core")
        storage_account_name = get_env("TERRAGRUNT_STORAGE_ACCOUNT", "itacommon")
        container_name       = "moetfstate"
        key                  = "${path_relative_to_include()}/terraform.tfstate"
    }
}

terraform {
    extra_arguments "core_vars" {
        commands = get_terraform_commands_that_need_vars()

        arguments = [
            "-var-file=./azure.tfvars"
        ]
    }
}