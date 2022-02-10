# Local Installation

## Login to GCR

Install `gcloud` SDK: [link](https://cloud.google.com/sdk/docs/install)

Initialize `gcloud` SDK (if you haven't already), with the account that has access to GCR.
```bash
gcloud init
```

If you already have `gcloud` initialized using a different account, then login with another one:

```bash
gcloud auth login
```

Next, to pull private images, you need to add Google credentials helper to your docker using Gcloud helper.

```bash
gcloud auth configure-docker
```

## MySQL

Create a secret with MySQL passwords, for example:
```bash
kubectl create secret generic mysql-auth \
    --from-literal=mysql-root-password='Oos-password1' \
    --from-literal=mysql-replication-password='Oos-password1' \
    --from-literal=mysql-password='Oos-password1'
```

Create a secret with MySQL password for Web API, for example:
```bash
kubectl create secret generic mysql-api-auth \
    --from-literal=API_PASSWORD='Oos-password1'
```

## Elasticsearch

Create a secret with Elasticsearch password, for example:
```bash
kubectl create secret generic elasticsearch-credentials \
    --from-literal=username=elastic \
    --from-literal=password='Oos-password1' \
    --from-literal=apipass='Oos-password1'
```

## MongoDB

Create a secret with MongoDB password, for example:
```bash
kubectl create secret generic mongodb-credentials \
    --from-literal=mongodb-passwords='Oos-password1' \
    --from-literal=mongodb-root-password='Oos-password1'
```

## Cert manager

First, install Cert Manager:
```
helm upgrade --install \
    --namespace cert-manager \
    --create-namespace \
    --set installCRDs=true \
    --set prometheus.enabled=false \
    cert-manager ./outofschool/charts/cert-manager-v1.6.0.tgz
```

## Nginx Ingress

Next, install Nginx Ingress controller:
```
helm upgrade --install \
    --namespace ingress-nginx \
    --create-namespace \
    --set config.proxy-body-size=50m \
    --set autoscaling.enabled=false \
    --set defaultBackend.enabled=false \
    ingress-nginx ./outofschool/charts/ingress-nginx-4.0.17.tgz
```

## Application

If needed, check `values-local.yaml` to see if you need to change anything.

Modify tags to the latest version (github tag short sha).

> On Windows/Mac Docker's Kubernetes can't access credentials using helper, so we need to pre-pull application images

Pull images:

```bash
docker pull gcr.io/gcp101292-pozashkillya/oos-api:LATEST_TAG
docker pull gcr.io/gcp101292-pozashkillya/oos-auth:LATEST_TAG
## Front is not working atm
# docker pull gcr.io/gcp101292-pozashkillya/oos-frontend:LATEST_TAG
```

Launch application:

helm upgrade --install \
    --namespace default \
    --values ./outofschool/values-local.yaml \
    outofschool ./outofschool/
```

If you need to switch tags, change the command to:

```bash
helm upgrade --install \
    --namespace default \
    --values ./outofschool/values-local.yaml \
    --set webapi.image.tag=LATEST_TAG \
    --set identity.image.tag=LATEST_TAG \
    --set frontend.image.tag=LATEST_TAG \
    outofschool ./outofschool/
```

Access Swagger UI at [http://localhost/web/swagger/index.html](http://localhost/web/swagger/index.html)
Access Kibana at: [http://localhost/kibana](http://localhost/kibana)