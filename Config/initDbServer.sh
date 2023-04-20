#!/bin/sh

docker run -d --name sql_server -e "ACCEPT_EULA=Y" -e MSSQL_COLLATION="Ukrainian_100_CI_AS" -e "SA_PASSWORD=Oos-password1" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest

