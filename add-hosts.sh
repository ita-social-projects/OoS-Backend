#!/usr/bin/env bash
grep "oos.local" /etc/hosts
if [ $? -eq 1 ];then
    echo "192.168.100.100 oos.local" | sudo tee -a /etc/hosts > /dev/null
fi