#!/usr/bin/env bash

function fail_if_error {
    [ $1 != 0 ] && {
        unset PASSPHRASE
        exit 3
    }
}

# Script accepts a FQDN as a single argument
DOMAIN="$1"
if [ -z "$DOMAIN" ]; then
    echo "Usage: $(basename $0) <domain>"
    exit 2
fi

if [ ! -d "./https" ]; then
    OPENSSL=$(command -v openssl)

    # Check if Openssl is installed
    if ! [ -x "$OPENSSL" ]; then
        echo 'Error: openssl is not installed.' >&2
        exit 1
    fi

    mkdir ./https
    # Generate a passphrase
    export PASSPHRASE=$(head -c 500 /dev/urandom | LC_CTYPE=C tr -dc 'a-z0-9A-Z' | head -c 128; echo)
    fail_if_error $?

    # Certificate details
    subj="
    C=UA
    ST=Kyiv
    L=Kyiv
    O=SS
    OU=ITA
    CN=$DOMAIN
    emailAddress=admin@$DOMAIN
    "

    openssl req \
        -x509 \
        -newkey rsa:2048 \
        -keyout ./https/${DOMAIN}.key \
        -out ./https/${DOMAIN}.crt \
        -days 365 \
        -subj "$(echo -n "$subj" | sed -e 's/^[[:space:]]*//' | tr "\n" "/")" \
        -passout env:PASSPHRASE

    fail_if_error $?

    openssl dhparam \
        -out ./https/dhparam.pem 2048 2> /dev/null
    fail_if_error $?

    openssl rsa \
        -in ./https/${DOMAIN}.key \
        -out ./https/${DOMAIN}.key \
        -passin env:PASSPHRASE
    fail_if_error $?

    if [[ $OSTYPE == 'darwin'* ]]; then
        sudo security find-certificate -a /Library/Keychains/System.keychain | awk -F'"' '/alis/{print $4}' | grep ${DOMAIN}
        if [ $? -eq 0 ];then
            sudo security delete-certificate -c ${DOMAIN} -t /Library/Keychains/System.keychain
        fi
        sudo security add-trusted-cert -r trustRoot -k /Library/Keychains/System.keychain ./https/${DOMAIN}.crt
    else
        if [[ -f "/usr/local/share/ca-certificates/${DOMAIN}.crt" ]]; then
            sudo rm /usr/local/share/ca-certificates/${DOMAIN}.crt
            sudo update-ca-certificates -f
        fi
        sudo cp ./https/${DOMAIN}.crt /usr/local/share/ca-certificates/
        sudo update-ca-certificates
    fi
fi