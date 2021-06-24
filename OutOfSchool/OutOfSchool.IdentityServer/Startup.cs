using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.EmailSender;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer
{
    public class Startup
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this.env = env;
            config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var connString = config["ConnectionStrings:DefaultConnection"];

            services
                .AddDbContext<OutOfSchoolDbContext>(options => options
                    .UseSqlServer(
                        connString,
                        optionsBuilder =>
                            optionsBuilder.MigrationsAssembly("OutOfSchool.IdentityServer")));

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<OutOfSchoolDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "IdentityServer.Cookie";
                config.LoginPath = "/Auth/Login";
                config.LogoutPath = "/Auth/Logout";
            });

            services.AddIdentityServer(options =>
                {
                    if (env.IsEnvironment("Release"))
                    {
                        options.IssuerUri = "http://hostname:5443";
                    }
                })
                .AddDeveloperSigningCredential()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(
                            connString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(
                            connString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddAspNetIdentity<User>()
                .AddProfileService<ProfileService>();

            services.AddEmailSender(
                builder => builder.Bind(config.GetSection(EmailOptions.SectionName)),
                builder => builder.Bind(config.GetSection(SmtpOptions.SectionName)));

            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var supportedCultures = new[]
            {
                  new CultureInfo("en"),
                  new CultureInfo("uk"),
            };

            var requestLocalization = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
            };

            app.UseRequestLocalization(requestLocalization);

            app.UseRouting();

            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
