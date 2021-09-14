#!/usr/bin/env bash

DBSTATUS=0
i=0

while [[ $DBSTATUS -eq 0 ]] && [[ $i -lt 60 ]]; do
	i=$(($i+10))
	RESULT=$(/opt/mssql-tools/bin/sqlcmd -h -1 -t 1 -U sa -P $SA_PASSWORD -Q "SET NOCOUNT ON; Select state_desc from sys.databases")
    if [[ $? -eq 0 ]];then
        echo -n $RESULT | grep "OFFLINE" -q
        DBSTATUS=$?
    fi
	sleep 10
done

if [[ $DBSTATUS -eq 0 ]]; then 
	echo "SQL Server took more than 60 seconds to start up or one or more databases are not in an ONLINE state"
	exit 1
fi

# Run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd \
    -v OOS_API_PASS=${OOS_API_PASS} \
    -v OOS_AUTH_PASS=${OOS_AUTH_PASS} \
    -S localhost \
    -U sa \
    -P $SA_PASSWORD \
    -d master \
    -i /usr/config/configure-db.sql
