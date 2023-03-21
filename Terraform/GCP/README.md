# Terraform Files

## Prereq

1. Install the [Cloud SDK](https://cloud.google.com/sdk/docs/install)
2. Install [Terraform](https://www.terraform.io/downloads.html)
3. Install [Terragrunt](https://terragrunt.gruntwork.io/docs/getting-started/install/)

## Deploy

1. Login to Gcloud CLI

```bash
gcloud auth login
```

3. Create `.tfvars` file with required data (see `gcp.tfvars.template` for required fields).

```bash
vi gcp.tfvars
```

4. Enable APIs

```bash
gcloud services enable \
    vpcaccess.googleapis.com \
    compute.googleapis.com \
    storage.googleapis.com \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    containerregistry.googleapis.com \
    cloudresourcemanager.googleapis.com \
    secretmanager.googleapis.com \
    iam.googleapis.com \
    servicenetworking.googleapis.com \
    sqladmin.googleapis.com \
    gmail.googleapis.com \
    dns.googleapis.com \
    oslogin.googleapis.com
```
5. Init Terraform
```
export TERRAGRUNT_PROJECT=your_project_id
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/key/admin.json"
terragrunt init
```

6. Apply infrastructure
```
terragrunt apply -target=module.k8s.helm_release.cert-manager
terragrunt apply
```

7. Install CSI Driver
[LINK](https://github.com/kubernetes-sigs/gcp-compute-persistent-disk-csi-driver/blob/master/docs/kubernetes/user-guides/driver-install.md)

If you CCM does not remove `network-unavailable` taint then run the following script:

```
kubectl proxy

for i in `kubectl get nodes -o jsonpath='{.items[*].metadata.name}'`; do 
    curl http://127.0.0.1:8001/api/v1/nodes/$i/status > a.json
    cat a.json | tr -d '\n' | sed 's/{[^}]\+NetworkUnavailable[^}]\+}/{"type": "NetworkUnavailable","status": "False","reason": "RouteCreated","message": "Manually set through k8s api"}/g' > b.json
    curl -X PUT http://127.0.0.1:8001/api/v1/nodes/$i/status -H "Content-Type: application/json" -d @b.json
done
```