USE [master]
GO

IF DB_ID('OutOfSchool') IS NOT NULL
  set noexec on               -- prevent creation when already exists

CREATE DATABASE [OutOfSchool];
GO

USE [OutOfSchool]
GO

ALTER DATABASE [OutOfSchool] COLLATE Ukrainian_100_CI_AS;

CREATE LOGIN [oos_api] WITH PASSWORD = '$(OOS_API_PASS)';

CREATE LOGIN [oos_auth] WITH PASSWORD = '$(OOS_AUTH_PASS)';

CREATE USER [oos_api] FOR LOGIN [oos_api] WITH DEFAULT_SCHEMA=[dbo];

CREATE USER [oos_auth] FOR LOGIN [oos_auth] WITH DEFAULT_SCHEMA=[dbo];
GO

-- TODO: Think about better permissions
EXEC sp_AddRoleMember 'db_datareader', 'oos_api';
EXEC sp_AddRoleMember 'db_datawriter', 'oos_api';
EXEC sp_AddRoleMember 'db_datareader', 'oos_auth';
EXEC sp_AddRoleMember 'db_datawriter', 'oos_auth';
EXEC sp_AddRoleMember 'db_ddladmin', 'oos_auth';