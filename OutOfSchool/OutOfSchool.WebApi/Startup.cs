using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning.ApiExplorer;
using AutoMapper;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Elasticsearch;
using Elastic.Apm.EntityFrameworkCore;
using Elastic.Apm.StackExchange.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HeaderPropagation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Primitives;
using OpenIddict.Validation.AspNetCore;
using OutOfSchool.BackgroundJobs.Config;
using OutOfSchool.BackgroundJobs.Extensions.Startup;
using OutOfSchool.BusinessLogic.Config.SearchString;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Elasticsearch;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.BusinessLogic.Services.Strategies.WorkshopStrategies;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Api.Files;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Services.Repository.Files;
using StackExchange.Redis;

namespace OutOfSchool.WebApi;

public static class Startup
{
    public static void Configure(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var httpRequest = context.Request;
            var httpResponse = context.Response;

            bool healthCheck = httpRequest.Path.Equals("/healthz/ready");

            int healthPort = app.Configuration.GetValue<int>("ApplicationPorts:HealthPort");

            if (httpRequest.HttpContext.Connection.LocalPort == healthPort && !healthCheck)
            {
                httpResponse.StatusCode = 404;
                return;
            }

            await next();
        });

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

        app.UseHeaderPropagation();

        app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("readiness"),
            AllowCachingResponses = false,
        })
            .RequireHost($"*:{app.Configuration.GetValue<int>("ApplicationPorts:HealthPort")}")
            .WithMetadata(new AllowAnonymousAttribute());

        app.MapHealthChecks("/healthz/active", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Name == "Liveness",
                ResponseWriter = async (context, _) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"status\":\"Healthy\"}");
                },
            })
            .WithMetadata(new AllowAnonymousAttribute());

        app.MapControllers();
        app.MapHub<ChatWorkshopHub>(Constants.PathToChatHub);
        app.MapHub<NotificationHub>(Constants.PathToNotificationHub);
    }

    public static async Task AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddElasticApmForAspNetCore(
            new HttpDiagnosticsSubscriber(),
            new EfCoreDiagnosticsSubscriber(),
            new ElasticsearchDiagnosticsSubscriber());

        services.Configure<AppDefaultsConfig>(configuration.GetSection(AppDefaultsConfig.Name));
        var identityConfig = configuration
            .GetSection(AuthorizationServerConfig.Name)
            .Get<AuthorizationServerConfig>();

        services.Configure<AuthorizationServerConfig>(configuration.GetSection(AuthorizationServerConfig.Name));
        services.Configure<EmployeeConfig>(configuration.GetSection(EmployeeConfig.Name));
        services.Configure<CommunicationConfig>(configuration.GetSection(CommunicationConfig.Name));
        services.Configure<GeocodingConfig>(configuration.GetSection(GeocodingConfig.Name));
        services.Configure<ParentConfig>(configuration.GetSection(ParentConfig.Name));

        services.AddMemoryCache();

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(identityConfig.Authority);
                options.AddAudiences(identityConfig.ClientId);
                options.UseIntrospection()
                    .SetClientId(identityConfig.ClientId)
                    .SetClientSecret(identityConfig.ClientSecret);

                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        services.AddCors(confg =>
            confg.AddPolicy(
                "AllowAll",
                p => p.WithOrigins(configuration["AllowedCorsOrigins"].Split(','))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

        var cacheProfilesConfigSection = configuration.GetSection(CacheProfilesConfig.Name);
        var cacheProfilesConfig = cacheProfilesConfigSection.Get<CacheProfilesConfig>();

        services.Configure<CacheProfilesConfig>(cacheProfilesConfigSection);

        services.AddControllers(options =>
            {
                options.CacheProfiles.Add(
                    Constants.CacheProfilePrivate,
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.Client,
                        NoStore = false,
                        Duration = cacheProfilesConfig.PrivateDurationInSeconds,
                    });
                options.CacheProfiles.Add(
                    Constants.CacheProfilePublic,
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.Any,
                        NoStore = false,
                        Duration = cacheProfilesConfig.PublicDurationInSeconds,
                    });
            })

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
                })
            .AddHeaderPropagation();

        services.AddRazorPages();
        services.AddHttpContextAccessor();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IMinistryAdminService, MinistryAdminService>();
        services.AddScoped<ISensitiveMinistryAdminService, MinistryAdminService>();
        services.AddScoped<IRegionAdminService, RegionAdminService>();
        services.AddScoped<IAreaAdminService, AreaAdminService>();

        services.AddScoped<ICommunicationService, CommunicationService>();

        // Images limits options
        services.Configure<ImagesLimits<Workshop>>(configuration.GetSection($"Images:{nameof(Workshop)}:Limits"));
        services.Configure<ImagesLimits<Teacher>>(configuration.GetSection($"Images:{nameof(Teacher)}:Limits"));
        services.Configure<ImagesLimits<Provider>>(configuration.GetSection($"Images:{nameof(Provider)}:Limits"));

        // Image options
        services.Configure<GcpStorageImagesSourceConfig>(configuration.GetSection(GcpStorageConfigConstants.GcpStorageImagesConfig));
        services.Configure<ExternalImageSourceConfig>(configuration.GetSection(ExternalImageSourceConfig.Name));
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

        services.AddTransient<BusinessEntityInterceptor>();
        services
            .AddDbContext<OutOfSchoolDbContext>((sp, options) => options
                .UseLazyLoadingProxies()
                .UseMySql(
                    connectionString,
                    serverVersion,
                    mySqlOptions =>
                        mySqlOptions
                            .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                            .EnableStringComparisonTranslations())
                .AddInterceptors(
                    sp.GetRequiredService<BusinessEntityInterceptor>()))
                .AddCustomDataProtection("WebApi");

        services.AddAutoMapper(typeof(CommonProfile), typeof(MappingProfile), typeof(ElasticProfile));

        // Add Elasticsearch client
        var elasticConfig = configuration
            .GetSection(ElasticConfig.Name)
            .Get<ElasticConfig>();
        services.Configure<ElasticConfig>(configuration.GetSection(ElasticConfig.Name));
        services.AddElasticsearch(elasticConfig);

        // ElasticPinger must precede ElasticIndexEnsureCreatedHostedService
        services.AddSingleton<ElasticPinger>();
        services.AddSingleton<IElasticsearchHealthService>(provider => provider.GetService<ElasticPinger>());
        services.AddHostedService<ElasticPinger>(provider => provider.GetService<ElasticPinger>());

        if (elasticConfig.EnsureIndex)
        {
            services.AddHostedService<ElasticIndexEnsureCreatedHostedService>();
        }

        services.AddTransient<IElasticsearchProvider<WorkshopES, WorkshopFilterES>, ESWorkshopProvider>();
        services.AddTransient<IElasticsearchService<WorkshopES, WorkshopFilterES>, ESWorkshopService>();

        // Search string options
        services.Configure<SearchStringOptions>(configuration.GetSection(nameof(SearchStringOptions)));
        services.AddScoped<ISearchStringService, SearchStringService>();

        // entities services
        services.AddTransient<IApplicationService, ApplicationService>();
        services.AddTransient<ISensitiveApplicationService, ApplicationService>();
        services.AddTransient<IChatMessageWorkshopService, ChatMessageWorkshopService>();
        services.AddTransient<IChatRoomWorkshopService, ChatRoomWorkshopService>();
        services.AddTransient<IChildService, ChildService>();
        services.AddTransient<IDirectionService, DirectionService>();
        services.AddTransient<ISensitiveDirectionService, DirectionService>();
        services.AddTransient<IFavoriteService, FavoriteService>();
        services.AddTransient<IParentService, ParentService>();
        services.AddTransient<IParentBlockedByAdminLogService, ParentBlockedByAdminLogService>();
        services.AddTransient<IPrivateProviderService, PrivateProviderService>();
        services.AddTransient<IProviderService, ProviderService>();
        services.AddTransient<ISensitiveProviderService, ProviderService>();
        services.AddTransient<IPublicProviderService, PublicProviderService>();
        services.AddTransient<IProviderTypeService, ProviderTypeService>();
        services.AddTransient<IProviderServiceV2, ProviderServiceV2>();
        services.AddTransient<IRatingService, RatingService>();
        services.AddTransient<IAverageRatingService, AverageRatingService>();
        services.AddTransient<ISocialGroupService, SocialGroupService>();
        services.AddTransient<ITagService, TagService>();
        services.AddTransient<IStatusService, StatusService>();
        services.AddTransient<IStatisticService, StatisticService>();
        services.AddTransient<ITeacherService, TeacherService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IValidationService, ValidationService>();
        services.AddTransient<IWorkshopService, WorkshopService>();
        services.AddTransient<ISensitiveWorkshopsService, WorkshopService>();
        services.AddTransient<IWorkshopServicesCombiner, WorkshopServicesCombiner>();
        services.AddTransient<IChangesLogService, ChangesLogService>();
        services.AddTransient<IValueProjector, ValueProjector>();
        services.AddTransient<IExternalExportProviderService, ExternalExportProviderService>();
        services.AddSingleton<ISendGridAccessibilityService, SendGridAccessibilityService>();
        services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

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
                ? new WorkshopESStrategy(
                    elasticSearchService,
                    sp.GetRequiredService<ILogger<WorkshopESStrategy>>(),
                    sp.GetRequiredService<IMapper>())
                : new WorkshopServiceStrategy(sp.GetRequiredService<IWorkshopService>(), sp.GetRequiredService<ILogger<WorkshopServiceStrategy>>());
        });

        // entities repositories
        services.AddTransient(typeof(IEntityAddOnlyRepository<,>), typeof(EntityRepository<,>));
        services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
        services.AddTransient(typeof(ISensitiveEntityRepository<>), typeof(SensitiveEntityRepository<>));

        services.AddTransient(typeof(IEntityRepositorySoftDeleted<,>), typeof(EntityRepositorySoftDeleted<,>));
        services.AddTransient(typeof(ISensitiveEntityRepositorySoftDeleted<>), typeof(SensitiveEntityRepositorySoftDeleted<>));

        services.AddTransient<IEmployeeRepository, EmployeeRepository>();
        services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
        services.AddTransient<IRegionAdminRepository, RegionAdminRepository>();
        services.AddTransient<IAreaAdminRepository, AreaAdminRepository>();

        services.AddTransient<IApplicationRepository, ApplicationRepository>();
        services
            .AddTransient<IChatRoomWorkshopModelForChatListRepository, ChatRoomWorkshopModelForChatListRepository
            >();
        services.AddTransient<IParentRepository, ParentRepository>();
        services.AddTransient<IProviderRepository, ProviderRepository>();
        services.AddTransient<IWorkshopRepository, WorkshopRepository>();

        // services.AddTransient<IExternalImageStorage, ExternalImageStorage>();
        var featuresConfig = configuration.GetSection(FeatureManagementConfig.Name).Get<FeatureManagementConfig>();
        var isImagesEnabled = featuresConfig.Images;
        var turnOnFakeStorage = configuration.GetValue<bool>("Images:TurnOnFakeImagesStorage") || !isImagesEnabled;
        services.AddImagesStorage(turnOnFakeStorage: turnOnFakeStorage);

        services.AddTransient<IElasticsearchSyncRecordRepository, ElasticsearchSyncRecordRepository>();
        services.AddTransient<INotificationRepository, NotificationRepository>();
        services.AddTransient<IStatisticReportRepository, StatisticReportRepository>();
        services.AddTransient<IFileInDbRepository, FileInDbRepository>();
        services.AddTransient<IBlockedProviderParentRepository, BlockedProviderParentRepository>();
        services.AddTransient<IChangesLogRepository, ChangesLogRepository>();
        var useFakeGeocoding = configuration.GetValue<bool>("GeoCoding:UseFakeGeocoder");
        if (useFakeGeocoding)
        {
            services.AddTransient<IGeocodingService, FakeGeocodingService>();
        }
        else
        {
            services.AddTransient<IGeocodingService, GeocodingService>();
        }

        services.AddTransient<IInstitutionHierarchyRepository, InstitutionHierarchyRepository>();
        services.AddTransient<IAchievementTypeService, AchievementTypeService>();
        services.AddTransient<IAchievementRepository, AchievementRepository>();
        services.AddTransient<IAchievementService, AchievementService>();
        services.AddTransient(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User);
        services.AddTransient<ICurrentUser, CurrentUserAccessor>();
        services.AddTransient<ICurrentUserService, CurrentUserService>();

        services.AddTransient<ICodeficatorRepository, CodeficatorRepository>();

        services.Configure<ChangesLogConfig>(configuration.GetSection(ChangesLogConfig.Name));

        services.AddTransient<ICompetitiveEventService, CompetitiveEventService>();

        services.AddTransient<IApiErrorService, ApiErrorService>();

        // Register the Permission policy handlers
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

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

        // MemoryCache options
        services.AddOptions<MemoryCacheConfig>()
            .Bind(configuration.GetSection(MemoryCacheConfig.Name))
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
        services.AddTransient<IEmployeeOperationsService, EmployeeOperationsRESTService>();

        // Required to inject it in OutOfSchool.WebApi.Extensions.Startup.CustomSwaggerOptions class
        services.AddSingleton(swaggerConfig);
        services.AddSwagger(swaggerConfig);

        services.AddProxy();

        var quartzConfig = configuration.GetSection(QuartzConfig.Name).Get<QuartzConfig>();
        services.AddDefaultQuartz(
            configuration,
            quartzConfig.ConnectionStringKey,
            q =>
        {
            if (!turnOnFakeStorage)
            {
                // TODO: for now this is not used in release
                q.AddGcpSynchronization(services, quartzConfig);
            }

            q.AddElasticsearchSynchronization(services, configuration);
            q.AddStatisticReportsCreating(services, quartzConfig);
            q.AddOldNotificationsClearing(services, quartzConfig);
            q.AddApplicationStatusChanging(quartzConfig);
            q.AddAverageRatingCalculating(services, quartzConfig);
            q.AddLicenseApprovalNotificationGenerating(services, quartzConfig);
            q.AddEmailSender(quartzConfig);
        });

        var isRedisEnabled = configuration.GetValue<bool>("Redis:Enabled");
        var redisConnection = $"{configuration.GetValue<string>("Redis:Server")}:{configuration.GetValue<int>("Redis:Port")},password={configuration.GetValue<string>("Redis:Password")}";

        var signalRBuilder = services.AddSignalR();
        var isAPMEnabled = configuration.GetValue<bool>("ElasticApm:Enabled");

        if (isRedisEnabled)
        {
            signalRBuilder.AddStackExchangeRedis(redisConnection, options =>
            {
                options.Configuration.AbortOnConnectFail = false;
                if (isAPMEnabled)
                {
                    options.ConnectionFactory = async writer =>
                    {
                        var connection = await ConnectionMultiplexer.ConnectAsync(options.Configuration, writer);
                        connection.UseElasticApm();
                        return connection;
                    };
                }
            });

            // TODO: Try to rework or remove if chat will stop working correctly
            services.AddSingleton(typeof(HubLifetimeManager<>), typeof(LocalDistributedHubLifetimeManager<>));
        }

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            if (isAPMEnabled)
            {
                options.ConnectionMultiplexerFactory = async () =>
                {
                    var connection = await ConnectionMultiplexer.ConnectAsync(redisConnection);
                    connection.UseElasticApm();
                    return connection;
                };
            }
        });

        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IMultiLayerCacheService, MultiLayerCache>();

        services.AddHealthChecks()
            .AddCheck("Liveness", () => HealthCheckResult.Healthy())
            .AddDbContextCheck<OutOfSchoolDbContext>(
                "Database",
                tags: ["readiness"]);

        Func<HeaderPropagationContext, StringValues> defaultHeaderDelegate = context =>
            StringValues.IsNullOrEmpty(context.HeaderValue) ? Guid.NewGuid().ToString() : context.HeaderValue;

        services.AddHeaderPropagation(options =>
        {
            options.Headers.Add("Request-Id", defaultHeaderDelegate);
            options.Headers.Add("X-Request-Id", defaultHeaderDelegate);
        });

        var mailConfig = configuration
            .GetSection(EmailOptions.SectionName)
            .Get<EmailOptions>();

        services.AddEmailSender(builder.Environment.IsDevelopment(), mailConfig.SendGridKey);
        services.AddEmailSenderService(builder => builder.Bind(configuration.GetSection(EmailOptions.SectionName)));

        // Hosts options
        services.Configure<HostsConfig>(configuration.GetSection(HostsConfig.Name));
    }
}
