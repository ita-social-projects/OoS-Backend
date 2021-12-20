#!/usr/bin/env bash

set -uo pipefail

# TODO: Do not delete, currently too expensive :)
# curl -sSO https://dl.google.com/cloudagents/add-monitoring-agent-repo.sh
# sudo bash add-monitoring-agent-repo.sh --also-install
# sudo service stackdriver-agent start

export INSTALL_K3S_VERSION=v1.21.6+k3s1
export K3S_DATASTORE_ENDPOINT="mysql://${db_username}:${db_password}@tcp(${db_host}:3306)/k3s"
export K3S_TOKEN=${token}

curl -sfL https://get.k3s.io | sh -s - server \
  --disable-cloud-controller \
  --write-kubeconfig-mode 644 \
  --kubelet-arg="cloud-provider=external" \
  --tls-san "${external_lb_ip_address}" \
  --tls-san "${external_hostname}" \
  --disable traefik \
  --no-deploy traefik \
  --disable servicelb \
  --no-deploy servicelb \
  --disable local-storage \
  --no-deploy local-storage

sleep 5

cat <<EOF | kubectl apply -f -
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: cloud-controller-manager
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: system:cloud-provider
rules:
- apiGroups:
  - ""
  resources:
  - events
  verbs:
  - create
  - patch
  - update
- apiGroups:
  - ""
  resources:
  - services/status
  verbs:
  - patch
  - update
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: system:cloud-provider
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: system:cloud-provider
subjects:
- kind: ServiceAccount
  name: cloud-provider
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: system:cloud-controller-manager
rules:
- apiGroups:
  - ""
  - events.k8s.io
  resources:
  - events
  verbs:
  - create
  - patch
  - update
- apiGroups:
  - coordination.k8s.io
  resources:
  - leases
  verbs:
  - create
- apiGroups:
  - coordination.k8s.io
  resourceNames:
    - cloud-controller-manager
  resources:
  - leases
  verbs:
  - get
  - update
- apiGroups:
  - ""
  resources:
  - endpoints
  - serviceaccounts
  verbs:
  - create
  - get
  - update
- apiGroups:
  - ""
  resources:
  - nodes
  verbs:
  - get
  - update
- apiGroups:
  - ""
  resources:
  - namespaces
  verbs:
  - get
- apiGroups:
  - ""
  resources:
  - nodes/status
  verbs:
  - patch
  - update
- apiGroups:
  - ""
  resources:
  - secrets
  verbs:
  - create
  - delete
  - get
  - update
- apiGroups:
  - "authentication.k8s.io"
  resources:
  - tokenreviews
  verbs:
  - create
- apiGroups:
  - "*"
  resources:
  - "*"
  verbs:
  - list
  - watch
- apiGroups:
  - ""
  resources:
  - serviceaccounts/token
  verbs:
  - create
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: system:controller:cloud-node-controller
rules:
- apiGroups:
  - ""
  resources:
  - events
  verbs:
  - create
  - patch
  - update
- apiGroups:
  - ""
  resources:
  - nodes
  verbs:
  - get
  - list
  - update
  - delete
  - patch
- apiGroups:
  - ""
  resources:
  - nodes/status
  verbs:
  - get
  - list
  - update
  - delete
  - patch
- apiGroups:
  - ""
  resources:
  - pods
  verbs:
  - list
  - delete
- apiGroups:
  - ""
  resources:
  - pods/status
  verbs:
  - list
  - delete
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: cloud-controller-manager:apiserver-authentication-reader
  namespace: kube-system
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: extension-apiserver-authentication-reader
subjects:
- apiGroup: ""
  kind: ServiceAccount
  name: cloud-controller-manager
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: system:cloud-controller-manager
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: system:cloud-controller-manager
subjects:
- kind: ServiceAccount
  name: cloud-controller-manager
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: system:controller:cloud-node-controller
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: system:controller:cloud-node-controller
subjects:
- kind: ServiceAccount
  name: cloud-node-controller
  namespace: kube-system
---
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: cloud-controller-manager
  namespace: kube-system
  labels:
    tier: control-plane
    k8s-app: cloud-controller-manager
spec:
  selector:
    matchLabels:
      k8s-app: cloud-controller-manager
  updateStrategy:
    type: RollingUpdate
  template:
    metadata:
      labels:
        tier: control-plane
        k8s-app: cloud-controller-manager
    spec:
      nodeSelector:
        node-role.kubernetes.io/master: "true"
      tolerations:
      - key: node.cloudprovider.kubernetes.io/uninitialized
        value: "true"
        effect: NoSchedule
      - key: node-role.kubernetes.io/master
        effect: NoSchedule
      securityContext:
        seccompProfile:
          type: RuntimeDefault
        runAsUser: 65521
        runAsNonRoot: true
      priorityClassName: system-node-critical
      hostNetwork: true
      serviceAccountName: cloud-controller-manager
      containers:
        - name: cloud-controller-manager
          image: quay.io/openshift/origin-gcp-cloud-controller-manager:4.10.0
          resources:
            requests:
              cpu: 50m
          command:
            - /bin/gcp-cloud-controller-manager
          args:
            - --bind-address=127.0.0.1
            - --cloud-provider=gce
            - --use-service-account-credentials
            - --configure-cloud-routes=false
            - --allocate-node-cidrs=false
            - --controllers=*,-nodeipam
          livenessProbe:
            httpGet:
              host: 127.0.0.1
              port: 10258
              path: /healthz
              scheme: HTTPS
            initialDelaySeconds: 15
            timeoutSeconds: 15
EOF

sleep 5
# Need to restart Kubelet after CCM install
sudo systemctl restart k3s

NAME=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/name" -H "Metadata-Flavor: Google")
ZONE=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/zone" -H "Metadata-Flavor: Google")
gcloud compute instances \
    add-labels $NAME \
    --zone $ZONE "--labels=startup-done=${random_number}"