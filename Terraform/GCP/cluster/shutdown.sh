#!/usr/bin/env bash

if [ -x "$(command -v k3s)" ]; then
    k3s stop
fi