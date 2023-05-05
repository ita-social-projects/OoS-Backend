#!/usr/bin/env bash

if [ -x "$(command -v k3s)" ]; then
    bash /usr/local/bin/k3s-agent-uninstall.sh
fi
