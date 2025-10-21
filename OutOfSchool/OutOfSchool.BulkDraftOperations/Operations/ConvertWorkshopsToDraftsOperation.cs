using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Extensions;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Common.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BulkDraftOperations.Operations;

public class ConvertWorkshopsToDraftsOperation : IConsoleOperation
{
    public string Name => "convert";
    public string Description => "Convert workshops created since a given date into drafts";

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
                services.AddHttpContextAccessor();
                services.AddTransient(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User);
                services.AddTransient<ICurrentUser, CurrentUserAccessor>();
                services.AddTransient<IContextAwareCurrentUser, ContextAwareCurrentUser>();
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
            });
    }

    public async Task<int> RunAsync(IHost host, string[] args, CancellationToken cancellationToken)
    {
        using var scope = host.Services.CreateScope();
        var logger =  scope.ServiceProvider.GetRequiredService<ILogger<ConvertWorkshopsToDraftsOperation>>();
        
        logger.LogInformation("Starting workshop to draft conversion...");
        var dbContext = scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>();

        var sinceArg = ArgsParser.GetArgValue(args, "since") ?? ArgsParser.GetArgValue(args, "date") ?? ArgsParser.GetArgValue(args, "after");
        DateTime cutoffUtc;
        if (!string.IsNullOrWhiteSpace(sinceArg))
        {
            if (DateOnly.TryParseExact(sinceArg!, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
            {
                cutoffUtc = DateTime.SpecifyKind(dateOnly.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                logger.LogInformation("Using cutoff date (UTC midnight): {CutoffUtc}", cutoffUtc);
            }
            else
            {
                logger.LogError("Invalid --since date. Expected format: yyyy-MM-dd");
                return 1;
            }
        }
        else
        {
            cutoffUtc = DateTime.UtcNow.AddHours(-6);
            logger.LogInformation("No --since provided. Using default cutoff: {CutoffUtc}", cutoffUtc);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            logger.LogInformation("Converting workshops to drafts...");
            
            var workshops = GetWorkshopsWithRelatedData(dbContext);

            var workshopDrafts = await workshops
                .Where(w => !dbContext.WorkshopDrafts.Any(wd => wd.WorkshopId == w.Id) && w.CreatedAt > cutoffUtc)
                .AsAsyncEnumerable()
                .Select(ConvertWorkshopToDraft)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {WorkshopDraftsCount} workshops to convert", workshopDrafts.Count);

            await dbContext.WorkshopDrafts.AddRangeAsync(workshopDrafts, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation("Transaction committed successfully. Converted {WorkshopDraftsCount} workshops to drafts", workshopDrafts.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Transaction rolled back due to error");
            throw;
        }

        return 0;
    }

    private static IQueryable<Workshop> GetWorkshopsWithRelatedData(OutOfSchoolDbContext dbContext)
    {
        return dbContext.Workshops
            .Include(w => w.Teachers)
            .Include(w => w.DateTimeRanges)
            .Include(w => w.WorkshopDescriptionItems)
            .IncludeContactsWithCodeficatorHierarchy()
            .Include(w => w.LanguageOfEducation)
            .Include(w => w.Images)
            .Include(w => w.Provider)
            .Include(w => w.InstitutionHierarchy)
            .ThenInclude(ih => ih.Institution)
            .Include(w => w.InstitutionHierarchy)
            .ThenInclude(ih => ih.SubDirections)
            .ThenInclude(sd => sd.Direction)
            .Where(w => !w.IsDeleted);
    }

    private static WorkshopDraft ConvertWorkshopToDraft(Workshop workshop)
    {
        var workshopV2Dto = workshop.ToV2Dto();

        var workshopDraft = workshopV2Dto.ToDraft();

        workshopDraft.DraftStatus = WorkshopDraftStatus.PendingModeration;
        workshopDraft.RejectionMessage = null;

        if (workshop.Images != null && workshop.Images.Count != 0)
        {
            workshopDraft.Images = workshop.Images
                .Select(originalImage => new Image<WorkshopDraft>
                {
                    EntityId = workshopDraft.Id,
                    ExternalStorageId = originalImage.ExternalStorageId
                })
                .ToList();
        }
        else
        {
            workshopDraft.Images = [];
        }

        return workshopDraft;
    }
}
