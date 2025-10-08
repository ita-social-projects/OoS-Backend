using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using MySqlConnector;
using OutOfSchool.BulkDraftOperations.Config;
using OutOfSchool.BulkDraftOperations.Extensions;
using OutOfSchool.BulkDraftOperations.Infrastructure;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.AverageRatings;
using OutOfSchool.BusinessLogic.Services.Elasticsearch;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.Logging;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.BusinessLogic.Services.Strategies.Interfaces;
using OutOfSchool.BusinessLogic.Services.Strategies.WorkshopStrategies;
using OutOfSchool.BusinessLogic.Services.SubordinationStructure;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.BusinessLogic.Services.Workshops;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Common.Models;
using OutOfSchool.ElasticsearchData;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.ExternalFileStore.Config;
using OutOfSchool.Redis;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Services.Repository.WorkshopDraftRepository;
using OutOfSchool.SportsRegistryApiClient.Extensions;
using Constants = OutOfSchool.Common.Constants;

namespace OutOfSchool.BulkDraftOperations.Operations;

public class ApproveWorkshopDraftsOperation : IConsoleOperation
{
    public string Name => "approve";
    public string Description => "Approve workshop drafts from a JSON file of { id } objects";

    public void ConfigureHost(IHostBuilder hostBuilder, string[] args)
    {
        hostBuilder
            .ConfigureServices((context, services) =>
            {
                var config = context.Configuration;

                var mariaDbServerVersion = config["MariaDbServerVersion"];
                var serverVersion = new MariaDbServerVersion(new Version(mariaDbServerVersion));
                if (serverVersion.Version.Major < Constants.MariaDbServerMinimalMajorVersion)
                {
                    throw new InvalidOperationException("MariaDb Server version should be 11 or higher.");
                }

                var connectionString = config.GetMySqlConnectionString<WebApiConnectionOptions>(
                    "DefaultConnection",
                    options => new MySqlConnectionStringBuilder
                    {
                        Server = options.Server,
                        Port = options.Port,
                        UserID = options.UserId,
                        Password = options.Password,
                        Database = options.Database,
                        GuidFormat = options.GuidFormat.ToEnum(MySqlGuidFormat.Default),
                        SslMode = options.SslMode.ToEnum(MySqlSslMode.None),
                    });

                services.AddFeatureManagement(config.GetSection(FeatureManagementConfig.Name));
                services.Configure<FeatureManagementConfig>(config.GetSection(FeatureManagementConfig.Name));

                services.AddHttpClient(config["Communication:ClientName"]);
                services.AddSportsRegistryClient(config);

                var storageConfig = config
                    .GetSection(StorageOptions.SectionName)
                    .Get<StorageOptions>();
                services.AddImagesStorage(storageConfig, true);

                services.Configure<UploadConcurrencySettings>(config.GetSection(nameof(UploadConcurrencySettings)));
                services.Configure<InstitutionOptions>(config.GetSection(InstitutionOptions.Name));
                services.Configure<ImageStorageOptions>(config.GetSection(ImageStorageOptions.Name));
                services.Configure<ImagesLimits<WorkshopDraft>>(config.GetSection($"Images:{nameof(Workshop)}:Limits"));
                services.Configure<ImageOptions<WorkshopDraft>>(config.GetSection($"Images:{nameof(Workshop)}:Specs"));

                services.AddHttpContextAccessor();
                services.AddTransient(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User);
                services.AddTransient<ICurrentUser, CurrentUserAccessor>();
                services.AddTransient<IContextAwareCurrentUser, ContextAwareCurrentUser>();
                services.AddTransient<IUserService, UserService>();
                services.AddTransient<TrackableEntityInterceptor>();

                services
                    .AddDbContext<OutOfSchoolDbContext>((sp, options) => options
                        .UseMySql(
                            connectionString,
                            serverVersion,
                            optionsBuilder =>
                                optionsBuilder
                                    .EnableStringComparisonTranslations()
                                    .UseMicrosoftJson())
                        .AddInterceptors(
                            sp.GetRequiredService<TrackableEntityInterceptor>()));

                services.AddSignalR();
                services.Configure<CommunicationConfig>(config.GetSection(CommunicationConfig.Name));

                services.AddTransient(typeof(IEntityAddOnlyRepository<,>), typeof(EntityRepository<,>));
                services.AddTransient(typeof(IEntityRepository<,>), typeof(EntityRepository<,>));
                services.AddTransient(typeof(ISensitiveEntityRepository<>), typeof(SensitiveEntityRepository<>));

                services.AddTransient(typeof(IEntityRepositorySoftDeleted<,>), typeof(EntityRepositorySoftDeleted<,>));
                services.AddTransient(typeof(ISensitiveEntityRepositorySoftDeleted<>), typeof(SensitiveEntityRepositorySoftDeleted<>));
                services.AddTransient<IWorkshopRepository, WorkshopRepository>();
                services.AddTransient<IWorkshopDraftRepository, WorkshopDraftRepository>();
                services.AddTransient<IInstitutionHierarchyRepository, InstitutionHierarchyRepository>();
                services.AddTransient<ICodeficatorRepository, CodeficatorRepository>();
                services.AddTransient<IProviderRepository, ProviderRepository>();
                services.AddTransient<IOfficialRepository, OfficialRepository>();
                services.AddTransient<IPositionRepository, PositionRepository>();
                services.AddTransient<IParentRepository, ParentRepository>();
                services.AddTransient<IInstitutionAdminRepository, InstitutionAdminRepository>();
                services.AddTransient<IApiErrorService, ApiErrorService>();
                services.AddTransient<IRegionAdminRepository, RegionAdminRepository>();
                services.AddTransient<IApplicationRepository, ApplicationRepository>();
                services.AddTransient<IChangesLogRepository, ChangesLogRepository>();
                services.AddTransient<IAreaAdminRepository, AreaAdminRepository>();
                services.AddTransient<IValueProjector, ValueProjector>();
                services.AddTransient<INestedObjectChangeLogger, NestedObjectChangeLogger>();
                services.AddTransient<ICollectionChangeLogger, CollectionChangeLogger>();
                services.AddScoped<ICommunicationService, CommunicationService>();
                services.AddTransient<INotificationRepository, NotificationRepository>();

                var elasticConfig = config
                    .GetSection(ElasticConfig.Name)
                    .Get<ElasticConfig>();
                services.Configure<ElasticConfig>(config.GetSection(ElasticConfig.Name));
                services.AddElasticsearch(elasticConfig);

                services.AddSingleton<ElasticPinger>();
                services.AddSingleton<IElasticsearchHealthService>(provider => provider.GetService<ElasticPinger>());
                services.AddTransient<IElasticsearchProvider<WorkshopES, WorkshopFilterES>, ESWorkshopProvider>();
                services.AddTransient<IElasticsearchService<WorkshopES, WorkshopFilterES>, ESWorkshopService>();
                services.AddTransient<IElasticsearchSynchronizationService<IWorkshopService, Workshop>, WorkshopSynchronizationService>();
                services.AddTransient<IElasticsearchSyncRecordRepository, ElasticsearchSyncRecordRepository>();
                services.AddTransient<IAddNewRecordToESSynchronizationTableService, AddNewRecordToESSynchronizationTableService>();
                services.AddTransient<IWorkshopStrategy>(sp =>
                {
                    var elasticSearchService = sp.GetRequiredService<IElasticsearchService<WorkshopES, WorkshopFilterES>>();
                    return elasticSearchService.IsElasticAlive
                        ? new WorkshopESStrategy(
                            elasticSearchService,
                            sp.GetRequiredService<ILogger<WorkshopESStrategy>>()
                        )
                        : new WorkshopServiceStrategy(sp.GetRequiredService<IWorkshopService>(), sp.GetRequiredService<ILogger<WorkshopServiceStrategy>>());
                });

                services.AddTransient<IStringLocalizer<SharedResource>, PassthroughStringLocalizer<SharedResource>>();

                services.AddTransient<ILanguageService, LanguageService>();
                services.AddTransient<IProviderService, ProviderService>();
                services.AddTransient<ICurrentUserService, CurrentUserService>();
                services.AddTransient<IWorkshopServicesCombinerV2, WorkshopServicesCombinerV2>();
                services.AddScoped<IRegionAdminService, RegionAdminService>();
                services.AddScoped<IMinistryAdminService, MinistryAdminService>();
                services.AddTransient<ICodeficatorService, CodeficatorService>();
                services.AddScoped<ISearchStringService, SearchStringService>();
                services.AddTransient<IChangesLogService, ChangesLogService>();
                services.AddTransient<IInstitutionHierarchyService, InstitutionHierarchyService>();
                services.AddTransient<IWorkshopServicesCombiner, WorkshopServicesCombiner>();
                services.AddTransient<IWorkshopService, WorkshopService>();
                services.AddTransient<ISensitiveWorkshopsService, WorkshopService>();
                services.AddTransient<ITeacherService, TeacherService>();
                services.AddTransient<IOperationWithObjectService, OperationWithObjectService>();
                services.AddTransient<ITagService, TagService>();
                services.AddScoped<IAreaAdminService, AreaAdminService>();
                services.AddTransient<INotificationService, NotificationService>();
                services.AddTransient(typeof(IContactsService<,>), typeof(ContactsService<,>));

                var redisConfig = config
                    .GetSection(RedisConfig.Name)
                    .Get<RedisConfig>();
                var redisConnection = redisConfig.GetRedisConnectionString();
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                });
                services.AddSingleton<ICacheService, CacheService>();
                services.AddTransient<IAverageRatingService, AverageRatingService>();
                services.AddTransient<IRatingService, RatingService>();

                services.AddScoped<IImageService, ImageService>();
                services.AddScoped<IImageValidator<WorkshopDraft>, ImageValidator<WorkshopDraft>>();
                services.AddScoped<IImageDependentEntityImagesInteractionService<WorkshopDraft>, ImageDependentEntityImagesInteractionService<WorkshopDraft>>();
                services.AddScoped<IImageDependentEntityImagesInteractionService<Workshop>, ImageDependentEntityImagesInteractionService<Workshop>>();
                services.AddScoped<IImageDependentEntityImagesInteractionService<Provider>, ImageDependentEntityImagesInteractionService<Provider>>();
                services.AddScoped<IEntityCoverImageInteractionService<TeacherDraft>, ImageDependentEntityImagesInteractionService<TeacherDraft>>();
                services.AddScoped<IEntityCoverImageInteractionService<Teacher>, ImageDependentEntityImagesInteractionService<Teacher>>();

                services.AddScoped<IWorkshopDraftService, WorkshopDraftService>();
                services.AddScoped<ISensitiveWorkshopDraftService, WorkshopDraftService>();
            });
    }

    public async Task<int> RunAsync(IHost host, string[] args, CancellationToken cancellationToken)
    {
        using var scope = host.Services.CreateScope();
        var logger =  scope.ServiceProvider.GetRequiredService<ILogger<ApproveWorkshopDraftsOperation>>();
        
        logger.LogInformation("Starting workshop draft approval...");
        var workshopDraftService = scope.ServiceProvider.GetRequiredService<IWorkshopDraftService>();

        var jsonFilePath = ArgsParser.GetArgValue(args, "file") ?? ArgsParser.GetArgValue(args, "f") ?? "workshops_approved.json";
        if (!File.Exists(jsonFilePath))
        {
            logger.LogError("JSON file not found: {JsonFilePath}", jsonFilePath);
            return 1;
        }

        logger.LogInformation("Reading workshop IDs from {JsonFilePath}...", jsonFilePath);
        var jsonContent = await File.ReadAllTextAsync(jsonFilePath, cancellationToken);

        var workshopData = JsonSerializer.Deserialize<List<WorkshopApprovalData>>(jsonContent);
        if (workshopData == null || !workshopData.Any())
        {
            logger.LogError("No workshop data found in JSON file");
            return 0;
        }

        var workshopIds = workshopData.Select(w => w.Id).ToList();
        logger.LogInformation("Found {WorkshopIdsCount} workshop IDs to approve", workshopIds.Count);

        var successCount = 0;
        var errorCount = 0;

        foreach (var workshopId in workshopIds)
        {
            try
            {
                logger.LogInformation("Approving workshop draft with ID: {WorkshopId}", workshopId);
                await workshopDraftService.Approve(Guid.Parse(workshopId));
                successCount++;
                logger.LogInformation("Successfully approved workshop draft: {WorkshopId}", workshopId);
            }
            catch (Exception ex)
            {
                errorCount++;
                logger.LogError(ex, "Error approving workshop draft {WorkshopId}", workshopId);
            }
        }

        logger.LogInformation("Approval process completed. Success: {SuccessCount}, Errors: {ErrorCount}", successCount, errorCount);
        return errorCount > 0 ? 2 : 0;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class WorkshopApprovalData
    {
        public string Id { get; set; } = string.Empty;
    }
}
