using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Repository.Files;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.AverageRatings;
using OutOfSchool.WebApi.Services.Strategies.Interfaces;
using OutOfSchool.WebApi.Services.Strategies.WorkshopStrategies;

namespace OutOfSchool.WebApi;

public static class Startup
{
    public static void Configure(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        var proxyOptions = app.Configuration.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
        app.UseProxy(proxyOptions);

        app.UseSecurityHttpHeaders(app.Environment.IsDevelopment());

        if (app.Environment.IsDevelopment())
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

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseSerilogRequestLogging();

        // Enable extracting token from QueryString for Hub-connection authorization
        app.UseMiddleware<AuthorizationTokenMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<ChatWorkshopHub>("/chathub/workshop");
        app.MapHub<NotificationHub>("/notificationhub");
    }

    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.Configure<AppDefaultsConfig>(configuration.GetSection(AppDefaultsConfig.Name));
        services.Configure<IdentityServerConfig>(configuration.GetSection(IdentityServerConfig.Name));
        services.Configure<ProviderAdminConfig>(configuration.GetSection(ProviderAdminConfig.Name));
        services.Configure<CommunicationConfig>(configuration.GetSection(CommunicationConfig.Name));
        services.Configure<GeocodingConfig>(configuration.GetSection(GeocodingConfig.Name));
        services.Configure<ParentConfig>(configuration.GetSection(ParentConfig.Name));

        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddAuthentication("Bearer")
            .AddIdentityServerAuthentication("Bearer", options =>
            {
                options.ApiName = "outofschoolapi";
                options.Authority = configuration["Identity:Authority"];

                options.RequireHttpsMetadata = false;
            });

        services.AddCors(confg =>
            confg.AddPolicy(
                "AllowAll",
                p => p.WithOrigins(configuration["AllowedCorsOrigins"].Split(','))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

        services.AddControllers().AddNewtonsoftJson()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddHttpClient(configuration["Communication:ClientName"])
            .AddHttpMessageHandler(handler =>
                new RetryPolicyDelegatingHandler(
                    int.Parse(configuration["Communication:MaxNumberOfRetries"])))
            .ConfigurePrimaryHttpMessageHandler(handler =>
                new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip,
                });

        services.AddHttpContextAccessor();
        services.AddScoped<IProviderAdminService, ProviderAdminService>();
        services.AddScoped<IMinistryAdminService, MinistryAdminService>();
        services.AddScoped<IRegionAdminService, RegionAdminService>();

        // Images limits options
        services.Configure<ImagesLimits<Workshop>>(configuration.GetSection($"Images:{nameof(Workshop)}:Limits"));
        services.Configure<ImagesLimits<Teacher>>(configuration.GetSection($"Images:{nameof(Teacher)}:Limits"));
        services.Configure<ImagesLimits<Provider>>(configuration.GetSection($"Images:{nameof(Provider)}:Limits"));

        // Image options
        services.Configure<GcpStorageImagesSourceConfig>(configuration.GetSection(GcpStorageConfigConstants.GcpStorageImagesConfig));
        services.Configure<ExternalImageSourceConfig>(configuration.GetSection(ExternalImageSourceConfig.Name));
        //services.AddSingleton<MongoDb>();
        services.Configure<ImageOptions<Workshop>>(configuration.GetSection($"Images:{nameof(Workshop)}:Specs"));
        services.Configure<ImageOptions<Teacher>>(configuration.GetSection($"Images:{nameof(Teacher)}:Specs"));
        services.Configure<ImageOptions<Provider>>(configuration.GetSection($"Images:{nameof(Provider)}:Specs"));

        // TODO: Move version check into an extension to reuse code across apps
        var mySQLServerVersion = configuration["MySQLServerVersion"];
        var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
        if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
        {
            throw new Exception("MySQL Server version should be 8 or higher.");
        }

        var connectionString = configuration.GetMySqlConnectionString<WebApiConnectionOptions>(
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

        services.AddDbContext<OutOfSchoolDbContext>(builder =>
                builder.UseLazyLoadingProxies().UseMySql(connectionString, serverVersion, mySqlOptions =>
                {
                    mySqlOptions
                        .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                        .EnableStringComparisonTranslations();
                }))
            .AddCustomDataProtection("WebApi");

        // Add Elasticsearch client
        var elasticConfig = configuration
            .GetSection(ElasticConfig.Name)
            .Get<ElasticConfig>();
        services.Configure<ElasticConfig>(configuration.GetSection(ElasticConfig.Name));
        services.AddElasticsearch(elasticConfig);
        services.AddTransient<IElasticsearchProvider<WorkshopES, WorkshopFilterES>, ESWorkshopProvider>();
        services.AddTransient<IElasticsearchService<WorkshopES, WorkshopFilterES>, ESWorkshopService>();

        // entities services
        services.AddTransient<IApplicationService, ApplicationService>();
        services.AddTransient<IChatMessageWorkshopService, ChatMessageWorkshopService>();
        services.AddTransient<IChatRoomWorkshopService, ChatRoomWorkshopService>();
        services.AddTransient<IChildService, ChildService>();
        services.AddTransient<IDirectionService, DirectionService>();
        services.AddTransient<IFavoriteService, FavoriteService>();
        services.AddTransient<IParentService, ParentService>();
        services.AddTransient<IProviderService, ProviderService>();
        services.AddTransient<IProviderTypeService, ProviderTypeService>();
        services.AddTransient<IProviderServiceV2, ProviderServiceV2>();
        services.AddTransient<IRatingService, RatingService>();
        services.AddTransient<IAverageRatingService, AverageRatingService>();
        services.AddTransient<ISocialGroupService, SocialGroupService>();
        services.AddTransient<IStatusService, StatusService>();
        services.AddTransient<IStatisticService, StatisticService>();
        services.AddTransient<ITeacherService, TeacherService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IValidationService, ValidationService>();
        services.AddTransient<IWorkshopService, WorkshopService>();
        services.AddTransient<IWorkshopServicesCombiner, WorkshopServicesCombiner>();
        services.AddTransient<IChangesLogService, ChangesLogService>();
        services.AddTransient<IValueProjector, ValueProjector>();

        services.AddTransient<IInstitutionHierarchyService, InstitutionHierarchyService>();
        services.AddTransient<IInstitutionService, InstitutionService>();
        services.AddTransient<IInstitutionFieldDescriptionService, InstitutionFieldDescriptionService>();

        services.AddTransient<IWorkshopServicesCombinerV2, WorkshopServicesCombinerV2>();
        services.AddTransient<IPermissionsForRoleService, PermissionsForRoleService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IImageValidator<Workshop>, ImageValidator<Workshop>>();
        services.AddScoped<IImageValidator<Teacher>, ImageValidator<Teacher>>();
        services.AddScoped<IImageValidator<Provider>, ImageValidator<Provider>>();
        services.AddTransient<ICompanyInformationService, CompanyInformationService>();

        services.AddScoped<IImageDependentEntityImagesInteractionService<Workshop>, ImageDependentEntityImagesInteractionService<Workshop>>();
        services.AddScoped<IImageDependentEntityImagesInteractionService<Provider>, ImageDependentEntityImagesInteractionService<Provider>>();
        services.AddScoped<IEntityCoverImageInteractionService<Teacher>, ImageDependentEntityImagesInteractionService<Teacher>>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IStatisticReportService, StatisticReportService>();
        services.AddTransient<IBlockedProviderParentService, BlockedProviderParentService>();
        services.AddTransient<ICodeficatorService, CodeficatorService>();
        services.AddTransient<IOperationWithObjectService, OperationWithObjectService>();

        services.AddTransient<IGRPCCommonService, GRPCCommonService>();
        services.AddTransient<IWorkshopStrategy>(sp =>
        {
            var elasticSearchService = sp.GetRequiredService<IElasticsearchService<WorkshopES, WorkshopFilterES>>();
            return elasticSearchService.IsElasticAlive
                ? new WorkshopESStrategy(elasticSearchService, sp.GetRequiredService<ILogger<WorkshopESStrategy>>())
                : new WorkshopServiceStrategy(sp.GetRequiredService<IWorkshopService>(), sp.GetRequiredService<ILogger<WorkshopServiceStrategy>>());
        });

        // entities repositories
        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
        services.AddTransient(typeof(ISensitiveEntityRepository<>), typeof(SensitiveEntityRepository<>));

        services.AddTransient<IProviderAdminRepository, ProviderAdminRepository>();
        services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
        services.AddTransient<IRegionAdminRepository, RegionAdminRepository>();

        services.AddTransient<IApplicationRepository, ApplicationRepository>();
        services
            .AddTransient<IChatRoomWorkshopModelForChatListRepository, ChatRoomWorkshopModelForChatListRepository
            >();
        services.AddTransient<IParentRepository, ParentRepository>();
        services.AddTransient<IProviderRepository, ProviderRepository>();
        services.AddTransient<IWorkshopRepository, WorkshopRepository>();
        //services.AddTransient<IExternalImageStorage, ExternalImageStorage>();
        services.AddImagesStorage(turnOnFakeStorage: configuration.GetValue<bool>("TurnOnFakeImagesStorage"));

        services.AddTransient<IElasticsearchSyncRecordRepository, ElasticsearchSyncRecordRepository>();
        services.AddTransient<INotificationRepository, NotificationRepository>();
        services.AddTransient<IStatisticReportRepository, StatisticReportRepository>();
        services.AddTransient<IFileInDbRepository, FileInDbRepository>();
        services.AddTransient<IBlockedProviderParentRepository, BlockedProviderParentRepository>();
        services.AddTransient<IChangesLogRepository, ChangesLogRepository>();
        services.AddTransient<IGeocodingService, GeocodingService>();


        services.AddTransient<IAchievementTypeService, AchievementTypeService>();
        services.AddTransient<IAchievementRepository, AchievementRepository>();
        services.AddTransient<IAchievementService, AchievementService>();
        services.AddTransient(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User);
        services.AddTransient<ICurrentUserService, CurrentUserService>();

        services.AddTransient<ICodeficatorRepository, CodeficatorRepository>();

        services.Configure<ChangesLogConfig>(configuration.GetSection(ChangesLogConfig.Name));

        // Register the Permission policy handlers
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        services.AddSingleton<ElasticPinger>();
        services.AddHostedService<ElasticPinger>(provider => provider.GetService<ElasticPinger>());

        services.AddSingleton(Log.Logger);
        services.AddVersioning();
        var swaggerConfig = configuration.GetSection(SwaggerConfig.Name).Get<SwaggerConfig>();

        // Add feature management
        services.AddFeatureManagement(configuration.GetSection(FeatureManagementConfig.Name));
        services.Configure<FeatureManagementConfig>(configuration.GetSection(FeatureManagementConfig.Name));

        // ApplicationsConstraints options
        services.AddOptions<ApplicationsConstraintsConfig>()
            .Bind(configuration.GetSection(ApplicationsConstraintsConfig.Name))
            .ValidateDataAnnotations();

        // Redis options
        services.AddOptions<RedisConfig>()
            .Bind(configuration.GetSection(RedisConfig.Name))
            .ValidateDataAnnotations();

        // StatisticReports
        var statisticReportsConfig = configuration.GetSection(StatisticReportConfig.Name).Get<StatisticReportConfig>();
        if (statisticReportsConfig.UseExternalStorage)
        {
            // use StorageSaverGoogle
        }
        else
        {
            services.AddTransient<IStatisticReportFileStorage, FileStatisticReportStorage>(provider
                => new FileStatisticReportStorage(provider.GetRequiredService<IFileInDbRepository>()));
        }

        // Notification options
        services.Configure<NotificationsConfig>(configuration.GetSection(NotificationsConfig.Name));

        // GRPC
        services.AddOptions<GRPCConfig>()
            .Bind(configuration.GetSection(GRPCConfig.Name))
            .ValidateDataAnnotations();

        var gRPCConfig = configuration.GetSection(GRPCConfig.Name).Get<GRPCConfig>();
        if (gRPCConfig.Enabled)
        {
            services.AddTransient<IProviderAdminOperationsService, ProviderAdminOperationsGRPCService>();
        }
        else
        {
            services.AddTransient<IProviderAdminOperationsService, ProviderAdminOperationsRESTService>();
        }

        // Required to inject it in OutOfSchool.WebApi.Extensions.Startup.CustomSwaggerOptions class
        services.AddSingleton(swaggerConfig);
        services.AddSwagger(swaggerConfig);

        services.AddProxy();

        services.AddAutoMapper(typeof(MappingProfile));

        var quartzConfig = configuration.GetSection(QuartzConfig.Name).Get<QuartzConfig>();
        services.AddDefaultQuartz(
            configuration,
            quartzConfig.ConnectionStringKey,
            q =>
        {
            q.AddGcpSynchronization(services, quartzConfig);
            q.AddElasticsearchSynchronization(services, configuration);
            q.AddStatisticReportsCreating(services, quartzConfig);
            q.AddOldNotificationsClearing(services, quartzConfig);
            q.AddApplicationStatusChanging(services, quartzConfig);
            q.AddAverageRatingCalculating(services, quartzConfig);
        });

        var isRedisEnabled = configuration.GetValue<bool>("Redis:Enabled");
        var redisConnection = $"{configuration.GetValue<string>("Redis:Server")}:{configuration.GetValue<int>("Redis:Port")},password={configuration.GetValue<string>("Redis:Password")}";

        var signalRBuilder = services.AddSignalR();

        if (isRedisEnabled)
        {
            signalRBuilder.AddStackExchangeRedis(redisConnection, options =>
            {
                options.Configuration.AbortOnConnectFail = false;
            });

            services.AddSingleton(typeof(HubLifetimeManager<>), typeof(LocalDistributedHubLifetimeManager<>));
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
        });

        services.AddSingleton<ICacheService, CacheService>();
    }
}
