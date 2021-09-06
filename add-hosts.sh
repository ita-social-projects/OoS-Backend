#!/usr/bin/env bash
# Script accepts an IP as a single argument
IP="$1"
if [ -z "$IP" ]; then
    echo "Usage: $(basename $0) <ip>"
    exit 2
fi

grep "oos.local" /etc/hosts
if [ $? -eq 1 ];then
    echo "${IP} oos.local" | sudo tee -a /etc/hosts > /dev/null
fi