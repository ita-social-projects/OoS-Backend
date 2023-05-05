#!/usr/bin/env bash

set -uo pipefail

# TODO: Do not delete, currently too expensive :)
# curl -sSO https://dl.google.com/cloudagents/add-monitoring-agent-repo.sh
# sudo bash add-monitoring-agent-repo.sh --also-install
# sudo service stackdriver-agent start

export INSTALL_K3S_VERSION=${k3s_version}
export K3S_TOKEN=${token}
NAME=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/name" -H "Metadata-Flavor: Google")
ZONE=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/zone" -H "Metadata-Flavor: Google")

curl -sfL https://get.k3s.io | sh -s - agent \
    --server "https://${main_node}:6443" \
    --kubelet-arg="cloud-provider=external" \
    --node-name $NAME

sleep 5

gcloud compute instances \
    add-tags $NAME \
    --tags=$NAME \
    --zone $ZONE
