using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Config;
using OutOfSchool.IdentityServer.KeyManagement;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using Serilog;

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

            services.AddCustomDataProtection("IdentityServer");

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

            services.ConfigureApplicationCookie(c =>
            {
                c.Cookie.Name = "IdentityServer.Cookie";
                c.LoginPath = "/Auth/Login";
                c.LogoutPath = "/Auth/Logout";
            });

            var issuerSection = config.GetSection(IssuerConfig.Name);
            services.Configure<IssuerConfig>(issuerSection);

            services.AddSingleton(Log.Logger);

            services.AddIdentityServer(options => { options.IssuerUri = issuerSection["Uri"]; })
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
                .AddProfileService<ProfileService>()
                .AddCustomKeyManagement<CertificateDbContext>(builder =>
                    builder.UseSqlServer(
                        connString,
                        sql => sql.MigrationsAssembly(migrationsAssembly)));

            services.AddEmailSender(
                builder => builder.Bind(config.GetSection(EmailOptions.SectionName)),
                builder => builder.Bind(config.GetSection(SmtpOptions.SectionName)));

            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            services.AddProxy();

            services.AddTransient<IParentRepository, ParentRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var proxyOptions = config.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
            app.UseProxy(proxyOptions);

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
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CustomRequestCultureProvider(context =>
                    {
                        if (!context.Request.Query.TryGetValue("ui-culture", out var selectedUiCulture) &
                            !context.Request.Query.TryGetValue("culture", out var selectedCulture))
                        {
                            var encodedPathAndQuery = context.Request.GetEncodedPathAndQuery();
                            var decodedUrl = WebUtility.UrlDecode(encodedPathAndQuery);
                            var dictionary = QueryHelpers.ParseQuery(decodedUrl);
                            dictionary.TryGetValue("ui-culture", out selectedUiCulture);
                            dictionary.TryGetValue("culture", out selectedCulture);
                        }

                        if (selectedCulture.FirstOrDefault() is null ^ selectedUiCulture.FirstOrDefault() is null)
                        {
                            return Task.FromResult(new ProviderCultureResult(
                                selectedCulture.FirstOrDefault() ??
                                selectedUiCulture.FirstOrDefault()));
                        }

                        return Task.FromResult(new ProviderCultureResult(
                            selectedCulture.FirstOrDefault(),
                            selectedUiCulture.FirstOrDefault()));
                    }),
                },
            };

            app.UseRequestLocalization(requestLocalization);

            app.UseRouting();

            app.UseCookiePolicy(new CookiePolicyOptions {MinimumSameSitePolicy = SameSiteMode.Lax});

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}