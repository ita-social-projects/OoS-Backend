#!/bin/mysqlsh --file
from mysqlsh import mysql
import os

# Export test database users using MySQL Shell

# Using classic session, so running only pure SQL
s = mysql.get_session('mysql://root@mysql:3306', os.getenv('ROOT_PASSWORD'))
users = s.run_sql("""SELECT User FROM mysql.user WHERE Host != 'localhost' AND User != 'root' AND User != ''""").fetch_all()
create_user_commands = []
grants = []
for user in users:
    for cmd in s.run_sql("""SHOW CREATE USER ?""", [user[0]]).fetch_all():
        create_user_commands.append(cmd[0])
    for cmd in s.run_sql("""SHOW GRANTS FOR ?""", [user[0]]).fetch_all():
        grants.append(cmd[0])

result = create_user_commands + grants + ['']
print(";\n".join(result))
