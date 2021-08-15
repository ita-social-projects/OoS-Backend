using System;
using System.Data.Common;
using System.Globalization;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using OutOfSchool.Common;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Extensions.Startup;
using OutOfSchool.WebApi.Hubs;
using OutOfSchool.WebApi.Middlewares;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;
using Serilog;

namespace OutOfSchool.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            var proxyOptions = Configuration.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
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
                DefaultRequestCulture = new RequestCulture("uk"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
            };

            app.UseRequestLocalization(requestLocalization);

            app.UseCors("AllowAll");

            app.UseMiddleware<ExceptionMiddlewareExtension>();

            app.UseSwaggerWithVersioning(provider, proxyOptions);

            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseSerilogRequestLogging();

            app.UseRouting();

            // Enable extracting token from QueryString for Hub-connection authorization
            app.UseMiddleware<AuthorizationTokenMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatWorkshopHub>("/chathub/workshop");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppDefaultsConfig>(Configuration.GetSection(AppDefaultsConfig.Name));
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.ApiName = "outofschoolapi";
                    options.Authority = Configuration["Identity:Authority"];

                    options.RequireHttpsMetadata = false;
                });

            services.AddCors(confg =>
                confg.AddPolicy(
                    "AllowAll",
                    p => p.WithOrigins(Configuration["AllowedCorsOrigins"].Split(','))
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()));

            services.AddControllers().AddNewtonsoftJson()
                .AddJsonOptions(options =>
                 options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;
            if (!connectionStringBuilder.ContainsKey("guidformat") || connectionStringBuilder["guidformat"].ToString().ToLower() != "binary16")
            {
                throw new Exception("The connection string should have a key: \"guidformat\" and a value: \"binary16\"");
            }

            var mySQLServerVersion = Configuration["MySQLServerVersion"];
            var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
            if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
            {
                throw new Exception("MySQL Server version should be 8 or higher.");
            }

            services.AddDbContext<OutOfSchoolDbContext>(builder =>
                builder.UseLazyLoadingProxies().UseMySql(connectionString, serverVersion))
                .AddCustomDataProtection("WebApi");

            // Add Elasticsearch client
            var elasticConfig = Configuration
                .GetSection(ElasticConfig.Name)
                .Get<ElasticConfig>();
            services.AddElasticsearch(elasticConfig);
            services.AddTransient<IElasticsearchProvider<WorkshopES, WorkshopFilterES>, ESWorkshopProvider>();
            services.AddTransient<IElasticsearchService<WorkshopES, WorkshopFilterES>, ESWorkshopService>();

            services.AddTransient<IElasticsearchSynchronizationService, ElasticsearchSynchronizationService>();

            // entities services
            services.AddTransient<IAddressService, AddressService>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IChatMessageWorkshopService, ChatMessageWorkshopService>();
            services.AddTransient<IChatRoomWorkshopService, ChatRoomWorkshopService>();
            services.AddTransient<IChildService, ChildService>();
            services.AddTransient<ICityService, CityService>();
            services.AddTransient<IClassService, ClassService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IDirectionService, DirectionService>();
            services.AddTransient<IFavoriteService, FavoriteService>();
            services.AddTransient<IParentService, ParentService>();
            services.AddTransient<IProviderService, ProviderService>();
            services.AddTransient<IRatingService, RatingService>();
            services.AddTransient<ISocialGroupService, SocialGroupService>();
            services.AddTransient<IStatusService, StatusService>();
            services.AddTransient<IStatisticService, StatisticService>();
            services.AddTransient<ITeacherService, TeacherService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IValidationService, ValidationService>();
            services.AddTransient<IWorkshopService, WorkshopService>();
            services.AddTransient<IWorkshopServicesCombiner, WorkshopServicesCombiner>();
            services.AddTransient<IPermissionsForRoleService, PermissionsForRoleService>();
            services.AddTransient<IBackupOperationService, BackupOperationService>();
            services.AddTransient<IBackupTrackerService, BackupTrackerService>();

            
            // entities repositories
            services.AddTransient<IEntityRepository<Address>, EntityRepository<Address>>();
            services.AddTransient<IEntityRepository<Application>, EntityRepository<Application>>();
            services.AddTransient<IEntityRepository<ChatMessageWorkshop>, EntityRepository<ChatMessageWorkshop>>();
            services.AddTransient<IEntityRepository<ChatRoomWorkshop>, EntityRepository<ChatRoomWorkshop>>();
            services.AddTransient<IEntityRepository<Child>, EntityRepository<Child>>();
            services.AddTransient<IEntityRepository<City>, EntityRepository<City>>();
            services.AddTransient<IEntityRepository<Favorite>, EntityRepository<Favorite>>();
            services.AddTransient<IEntityRepository<Direction>, EntityRepository<Direction>>();
            services.AddTransient<IEntityRepository<SocialGroup>, EntityRepository<SocialGroup>>();
            services.AddTransient<IEntityRepository<InstitutionStatus>, EntityRepository<InstitutionStatus>>();
            services.AddTransient<IEntityRepository<Teacher>, EntityRepository<Teacher>>();
            services.AddTransient<IEntityRepository<User>, EntityRepository<User>>();
            services.AddTransient<IEntityRepository<PermissionsForRole>, EntityRepository<PermissionsForRole>>();
            services.AddTransient<IEntityRepository<BackupOperation>, EntityRepository<BackupOperation>>();
            services.AddTransient<IEntityRepository<BackupTracker>, EntityRepository<BackupTracker>>();
            services.AddTransient<IEntityRepository<ElasticsearchSyncRecord>, EntityRepository<ElasticsearchSyncRecord>>();

            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IChatRoomWorkshopModelForChatListRepository, ChatRoomWorkshopModelForChatListRepository>();
            services.AddTransient<IClassRepository, ClassRepository>();
            services.AddTransient<IDepartmentRepository, DepartmentRepository>();
            services.AddTransient<IParentRepository, ParentRepository>();
            services.AddTransient<IProviderRepository, ProviderRepository>();
            services.AddTransient<IRatingRepository, RatingRepository>();
            services.AddTransient<IWorkshopRepository, WorkshopRepository>();

            //Register the Permission policy handlers
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            services.AddSingleton<ElasticPinger>();
            services.AddHostedService<ElasticPinger>(provider => provider.GetService<ElasticPinger>());

            services.AddSingleton(Log.Logger);
            services.AddVersioning();
            var swaggerConfig = Configuration.GetSection(SwaggerConfig.Name).Get<SwaggerConfig>();

            // Add feature management
            services.AddFeatureManagement(Configuration.GetSection(Constants.SectionName));
            services.AddSingleton<IConfiguration>(Configuration);

            // Required to inject it in OutOfSchool.WebApi.Extensions.Startup.CustomSwaggerOptions class
            services.AddSingleton(swaggerConfig);
            services.AddSwagger(swaggerConfig);

            services.AddProxy();

            services.AddAutoMapper(typeof(MappingProfile));

            services.AddSignalR();
        }
    }
}