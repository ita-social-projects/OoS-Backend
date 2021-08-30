#!/usr/bin/env bash

# Script accepts a FQDN as a single argument
DOMAIN="$1"
if [ -z "$DOMAIN" ]; then
    echo "Usage: $(basename $0) <domain>"
    exit 2
fi


if [[ $OSTYPE == 'darwin'* ]]; then
    sudo security find-certificate -a /Library/Keychains/System.keychain | awk -F'"' '/alis/{print $4}' | grep ${DOMAIN}
    if [ $? -eq 0 ];then
        sudo security delete-certificate -c ${DOMAIN} -t /Library/Keychains/System.keychain
    fi
else
    if [[ -f "/usr/local/share/ca-certificates/${DOMAIN}.crt" ]]; then
        sudo rm /usr/local/share/ca-certificates/${DOMAIN}.crt
        sudo update-ca-certificates -f
    fi
fi
if [ -d "./https" ]; then
    rm -rf ./https
fi