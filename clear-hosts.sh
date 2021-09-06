#!/usr/bin/env bash
if [[ $OSTYPE == 'darwin'* ]]; then
    sudo sed -i '' "/oos.local/d" /etc/hosts
else
    sudo sed -i "/oos.local/d" /etc/hosts
fi