using System.Text.Json.Serialization;
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

        services.AddScoped<IProviderAdminService, ProviderAdminService>();
        services.AddScoped<IMinistryAdminService, MinistryAdminService>();

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
        services.AddTransient<IProviderServiceV2, ProviderServiceV2>();
        services.AddTransient<IRatingService, RatingService>();
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
        services.AddTransient<IBlockedProviderParentService, BlockedProviderParentService>();
        services.AddTransient<ICodeficatorService, CodeficatorService>();
        services.AddTransient<IGRPCCommonService, GRPCCommonService>();
        services.AddTransient<IWorkshopStrategy>(sp =>
        {
            var elasticSearchService = sp.GetRequiredService<IElasticsearchService<WorkshopES, WorkshopFilterES>>();
            return elasticSearchService.IsElasticAlive
                ? new WorkshopESStrategy(elasticSearchService, sp.GetRequiredService<ILogger<WorkshopESStrategy>>())
                : new WorkshopServiceStrategy(sp.GetRequiredService<IWorkshopService>(), sp.GetRequiredService<ILogger<WorkshopServiceStrategy>>());
        });

        // entities repositories
        services.AddTransient<IEntityRepository<long, Address>, EntityRepository<long, Address>>();
        services.AddTransient<IEntityRepository<Guid, Application>, EntityRepository<Guid, Application>>();
        services.AddTransient<IEntityRepository<Guid, ChatMessageWorkshop>, EntityRepository<Guid, ChatMessageWorkshop>>();
        services.AddTransient<IEntityRepository<Guid, ChatRoomWorkshop>, EntityRepository<Guid, ChatRoomWorkshop>>();
        services.AddTransient<IEntityRepository<Guid, Child>, ChildRepository>();
        services.AddTransient<IEntityRepository<long, City>, EntityRepository<long, City>>();
        services.AddTransient<IEntityRepository<long, Favorite>, EntityRepository<long, Favorite>>();
        services.AddTransient<IEntityRepository<long, SocialGroup>, EntityRepository<long, SocialGroup>>();
        services.AddTransient<IEntityRepository<long, InstitutionStatus>, EntityRepository<long, InstitutionStatus>>();
        services.AddTransient<ISensitiveEntityRepository<Teacher>, SensitiveEntityRepository<Teacher>>();
        services.AddTransient<IEntityRepository<string, User>, EntityRepository<string, User>>();
        services.AddTransient<IEntityRepository<long, PermissionsForRole>, EntityRepository<long, PermissionsForRole>>();

        services.AddTransient<IProviderAdminRepository, ProviderAdminRepository>();
        services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
        services.AddTransient<ISensitiveEntityRepository<CompanyInformation>, SensitiveEntityRepository<CompanyInformation>>();
        services.AddTransient<ISensitiveEntityRepository<CompanyInformationItem>, SensitiveEntityRepository<CompanyInformationItem>>();

        services.AddTransient<IApplicationRepository, ApplicationRepository>();
        services
            .AddTransient<IChatRoomWorkshopModelForChatListRepository, ChatRoomWorkshopModelForChatListRepository
            >();
        services.AddTransient<IClassRepository, ClassRepository>();
        services.AddTransient<IDepartmentRepository, DepartmentRepository>();
        services.AddTransient<IDirectionRepository, DirectionRepository>();
        services.AddTransient<IParentRepository, ParentRepository>();
        services.AddTransient<IProviderRepository, ProviderRepository>();
        services.AddTransient<IRatingRepository, RatingRepository>();
        services.AddTransient<IWorkshopRepository, WorkshopRepository>();
        //services.AddTransient<IExternalImageStorage, ExternalImageStorage>();
        services.AddImagesStorage(turnOnFakeStorage: configuration.GetValue<bool>("TurnOnFakeImagesStorage"));

        services.AddTransient<IElasticsearchSyncRecordRepository, ElasticsearchSyncRecordRepository>();
        services.AddTransient<INotificationRepository, NotificationRepository>();
        services.AddTransient<IBlockedProviderParentRepository, BlockedProviderParentRepository>();
        services.AddTransient<IChangesLogRepository, ChangesLogRepository>();
        services.AddTransient<IEntityRepository<long, ProviderAdminChangesLog>, EntityRepository<long, ProviderAdminChangesLog>>();

        services.AddTransient<IEntityRepository<long, AchievementType>, EntityRepository<long, AchievementType>>();
        services.AddTransient<IAchievementTypeService, AchievementTypeService>();
        services.AddTransient<IEntityRepository<long, AchievementTeacher>, EntityRepository<long, AchievementTeacher>>();
        services.AddTransient<IAchievementRepository, AchievementRepository>();
        services.AddTransient<IAchievementService, AchievementService>();

        // Institution hierarchy
        services.AddTransient<ISensitiveEntityRepository<Institution>, SensitiveEntityRepository<Institution>>();
        services.AddTransient<ISensitiveEntityRepository<InstitutionFieldDescription>, SensitiveEntityRepository<InstitutionFieldDescription>>();
        services.AddTransient<ISensitiveEntityRepository<InstitutionHierarchy>, SensitiveEntityRepository<InstitutionHierarchy>>();

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

        services.AddSignalR();

        var quartzConfig = configuration.GetSection(QuartzConfig.Name).Get<QuartzConfig>();
        services.AddDefaultQuartz(
            configuration,
            quartzConfig.ConnectionStringKey,
            q =>
        {
            q.AddGcpSynchronization(services, quartzConfig);
            q.AddElasticsearchSynchronization(services, configuration);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration =
                $"{configuration.GetValue<string>("Redis:Server")}:{configuration.GetValue<int>("Redis:Port")},password={configuration.GetValue<string>("Redis:Password")}";
        });

        services.AddSingleton<ICacheService, CacheService>();
    }
}
