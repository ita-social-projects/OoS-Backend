#!/usr/bin/env bash

NAME=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/name" -H "Metadata-Flavor: Google")
ZONE=$(curl "http://metadata.google.internal/computeMetadata/v1/instance/zone" -H "Metadata-Flavor: Google")
# kubectl drain $NAME --ignore-daemonsets --delete-emptydir-data
gcloud compute instances remove-labels $NAME --zone $ZONE --labels=startup-done

if [ -x "$(command -v k3s)" ]; then
    bash /usr/local/bin/k3s-killall.sh
fi
