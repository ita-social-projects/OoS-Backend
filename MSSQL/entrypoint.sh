#!/usr/bin/env bash

# Start the script to create the DB and user
/bin/bash /usr/config/configure-db.sh &

# Start SQL Server
/opt/mssql/bin/sqlservr
