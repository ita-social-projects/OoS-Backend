# Namespace

Create deployemtn namespace:
```bash
kubectl create ns outofschool
```
# MySQL

Create a secret with MySQL passwords, for example:
```bash
kubectl create secret generic mysql-auth \
    --from-literal=mysql-root-password='Oos-password1' \
    --from-literal=mysql-replication-password='Oos-password1' \
    --from-literal=mysql-password='Oos-password1' \
    -n outofschool
```

Create a secret with MySQL password for Web API, for example:
```bash
kubectl create secret generic mysql-api-auth \
    --from-literal=API_PASSWORD='Oos-password1' \
    -n outofschool
```

# Elasticsearch

Create a secret with Elasticsearch password, for example:
```bash
kubectl create secret generic elasticsearch-credentials \
    --from-literal=username=elastic \
    --from-literal=password='Oos-password1' \
    --from-literal=apipass='Oos-password1' \
    -n outofschool
```

# MongoDB

Create a secret with MongoDB password, for example:
```bash
kubectl create secret generic mongodb-credentials \
    --from-literal=mongodb-passwords='Oos-password1' \
    --from-literal=mongodb-root-password='Oos-password1' \
    -n outofschool
```