#!/usr/bin/env bash

# Script accepts a FQDN as a single argument
DOMAIN="$1"
if [ -z "$DOMAIN" ]; then
    echo "Usage: $(basename $0) <domain>"
    exit 2
fi


if [[ $OSTYPE == 'darwin'* ]]; then
    echo "TODO:"
else
    if [[ -f "/usr/local/share/ca-certificates/${DOMAIN}.crt" ]]; then
        sudo rm /usr/local/share/ca-certificates/${DOMAIN}.crt
        sudo update-ca-certificates -f
    fi
fi
if [ -d "./https" ]; then
    rm -rf ./https
fi