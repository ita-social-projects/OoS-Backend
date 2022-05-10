#!/usr/bin/env bash

set -uo pipefail

# TODO: Do not delete, currently too expensive :)
# curl -sSO https://dl.google.com/cloudagents/add-monitoring-agent-repo.sh
# sudo bash add-monitoring-agent-repo.sh --also-install
# sudo service stackdriver-agent start

export INSTALL_K3S_VERSION=v1.21.12+k3s1
export K3S_TOKEN=${token}

curl -sfL https://get.k3s.io | sh -s - agent \
    --server "https://${main_node}:6443" \
    --kubelet-arg="cloud-provider=external"

sleep 5

NAME=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/name" -H "Metadata-Flavor: Google")
ZONE=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/zone" -H "Metadata-Flavor: Google")
kubectl uncordon $NAME
gcloud compute instances \
    add-tags $NAME \
    --tags=$NAME \
    --zone $ZONE
gcloud compute instances \
    add-labels $NAME \
    --zone $ZONE "--labels=startup-done=${random_number}"