# Terraform Files

## DEPRECATION WARNING
Does not work after switching to MySQL.

## Prereq

1. Install the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
2. Install [Terraform](https://www.terraform.io/downloads.html)
3. Install [Terragrunt](https://terragrunt.gruntwork.io/docs/getting-started/install/)

## Deploy

1. Login to Azure in CLI

```bash
az login
```

2. *Optional* if you have more than 1 subscription select the one you want to use as default

```bash
az account set --subscription YOUR_SUBSCRIPTION_ID
```

3. Create `.tfvars` file with required `subscription_id` & `tenant_id`, and optional `admin_ip` with your IP.

```bash
vi azure.tfvars
```

4. Init Terraform
```
terragrunt init
```

5. Apply infrastructure
```
terragrunt apply
```

6. Get the git repo commands from outputs
