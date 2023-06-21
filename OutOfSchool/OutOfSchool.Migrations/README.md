# Migrations commands

## Install EF Core tools

```bash
dotnet tool install --global dotnet-ef
```

## Initial auth servers db config

### Identity Server

```bash
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
```

Update the database:

```bash
dotnet ef database update -c PersistedGrantDbContext
dotnet ef database update -c ConfigurationDbContext
```

### OpenIdDict

```bash
dotnet ef migrations add AddOpenIdDict -c OpenIdDictDbContext -o Data/Migrations/OpenIdDictMigrations
```

## Create migrations

```bash
dotnet ef migrations add AddressH3Add -c OutOfSchoolDbContext -o Data/Migrations/OutOfSchoolMigrations
```

### Update database

```bash
dotnet ef database update -c OutOfSchoolDbContext
```

#### Update DB to the specified migration

```bash
dotnet ef database update 20220130210817_AddNotifications -c OutOfSchoolDbContext
```

#### Remove latest migration
```bash
dotnet ef migrations remove -c OutOfSchoolDbContext
```

## Return list of migrations with theirs status
```bash
dotnet ef migrations list -c OutOfSchoolDbContext
```

## Build migration bundle

Runtimes:
* `osx-64`
* `linux-x64`
* `win-x64`

depending on your system, example is `osx`.

```bash
dotnet ef migrations bundle --configuration Bundle --context OutOfSchoolDbContext --target-runtime=osx-x64 -o efbundle
```