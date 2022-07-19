using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.OpenIddict;
using OutOfSchool.OpenIddict.Config;
using OutOfSchool.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

// Add services to the container.
services.AddControllersWithViews();

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/account/login";
    });

var connectionString = config.GetMySqlConnectionString<IdentityConnectionOptions>(
            "DefaultConnection",
    options => new MySqlConnectionStringBuilder
    {
        Server = options.Server,
        Port = options.Port,
        UserID = options.UserId,
        Password = options.Password,
        Database = options.Database,
        GuidFormat = options.GuidFormat.ToEnum(MySqlGuidFormat.Default),
    });

var mySQLServerVersion = config["MySQLServerVersion"];
var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
{
    throw new Exception("MySQL Server version should be 8 or higher.");
}

services
    .AddDbContext<OutOfSchoolDbContext>(options =>
    {
        options.UseMySql(
            connectionString,
            serverVersion,
            optionsBuilder =>
                optionsBuilder
                    .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                    .MigrationsAssembly("OutOfSchool.IdentityServer"));

        // Register the entity sets needed by OpenIddict.
        options.UseOpenIddict();
    });

//services.AddDbContext<DbContext>(options =>
//{
//    // Configure the context to use an in-memory store.
//    options.UseInMemoryDatabase(nameof(DbContext));

//    // Register the entity sets needed by OpenIddict.
//    options.UseOpenIddict();
//});

services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the EF Core stores/models.
        options.UseEntityFrameworkCore()
            .UseDbContext<OutOfSchoolDbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        options
            .AllowClientCredentialsFlow()
            .AllowAuthorizationCodeFlow()
                .RequireProofKeyForCodeExchange()
            .AllowRefreshTokenFlow();

        options
            .SetTokenEndpointUris("/connect/token")
            .SetAuthorizationEndpointUris("/connect/authorize")
            .SetUserinfoEndpointUris("/connect/userinfo");

        // Encryption and signing of tokens
        options
            .AddEphemeralEncryptionKey()
            .AddEphemeralSigningKey()
            .DisableAccessTokenEncryption();

        // Register scopes (permissions)
        options.RegisterScopes("api");

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough();
    });

services.AddHostedService<TestData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();
