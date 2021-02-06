docker run -d --name azuresql-edge -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Oos-password1" -p 1433:1433 -d mcr.microsoft.com/azure-sql-edge
pause
