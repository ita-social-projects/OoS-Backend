using IdentityServer4.Test;

namespace OutOfSchool.IdentityServer.Extensions;
public static class ConfigureIdentityExtensions
{
    public static void ConfigureIdentity(
        this IServiceCollection services,
        string connectionString,
        string issuerUri,
        MySqlServerVersion serverVersion,
        string migrationsAssembly)
    {
        services.AddIdentity<User, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<OutOfSchoolDbContext>()
            .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);

        services.AddIdentityServer(options => { options.IssuerUri = issuerUri; })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseMySql(
                        connectionString,
                        serverVersion,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder =>
                    builder.UseMySql(
                        connectionString,
                        serverVersion,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 3600;
            })
            .AddAspNetIdentity<User>()
            .AddProfileService<ProfileService>()
            .AddCustomKeyManagement<CertificateDbContext>(builder =>
                builder.UseMySql(
                    connectionString,
                    serverVersion,
                    sql => sql.MigrationsAssembly(migrationsAssembly)));
    }
}
