#nullable enable
namespace OutOfSchool.IdentityServer.Extensions;

public static class ConfigureIdentityExtensions
{
    public static void ConfigureIdentity(
        this IServiceCollection services,
        string connectionString,
        string issuerUri,
        MySqlServerVersion serverVersion,
        string migrationsAssembly,
        Action<IdentityBuilder>? msIdentityConfig = null,
        Action<IIdentityServerBuilder>? identityServerConfig = null)
    {
        var identityBuilder = services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<OutOfSchoolDbContext>();

        msIdentityConfig?.Invoke(identityBuilder);

        var identityServerBuilder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = issuerUri;
                options.UserInteraction.LoginUrl = "~/Auth/Login";
                options.UserInteraction.LogoutUrl = "~/Auth/Logout";
                options.UserInteraction.LoginReturnUrlParameter = "ReturnUrl";
                options.Authentication.CookieAuthenticationScheme = IdentityConstants.ApplicationScheme;
            })
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
            .AddCustomKeyManagement<CertificateDbContext>(builder =>
                builder.UseMySql(
                    connectionString,
                    serverVersion,
                    sql => sql.MigrationsAssembly(migrationsAssembly)));

        identityServerConfig?.Invoke(identityServerBuilder);
    }
}
