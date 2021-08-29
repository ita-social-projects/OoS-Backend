#!/usr/bin/env bash

if [ "$IN_VAGRANT" ]; then
    cd /vagrant
fi 

COMPOSE=$(command -v docker-compose)

M1=''
if [[ `uname -m` == 'arm64' ]]; then
    M1='-f docker-compose.m1.yml'
fi

if ! [ -x "$COMPOSE" ]; then
    echo 'Error: Docker compose is not installed.' >&2
    exit 1
fi

# TODO: Figure out the reason for extra SIGINT/SIGTERM on OSX
TAG=$(git rev-parse --short HEAD) $COMPOSE \
    -f docker-compose.yml \
    -f docker-compose.local.yml \
    ${M1} up -d
