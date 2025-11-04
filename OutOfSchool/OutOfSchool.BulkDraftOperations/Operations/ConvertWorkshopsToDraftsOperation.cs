using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Extensions;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Common.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BulkDraftOperations.Operations;

public class ConvertWorkshopsToDraftsOperation : IConsoleOperation
{
    public string Name => "convert";
    public string Description => "Convert workshops or competitive events created since a given date into drafts";

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
        
        logger.LogInformation("Starting conversion to drafts...");
        var dbContext = scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>();

        var entityArg = ArgsParser.GetArgValue(args, "entity")
                       ?? ArgsParser.GetArgValue(args, "type")
                       ?? ArgsParser.GetArgValue(args, "target");

        var entityNormalized = entityArg?.Trim().ToLowerInvariant();
        var isWorkshops = entityNormalized is "workshop" or "workshops" or "ws";
        var isCompetitions = entityNormalized is "competition" or "competitions" or "ce" or "competitive-events" or "event" or "events";

        if (!isWorkshops && !isCompetitions)
        {
            logger.LogError("Missing or invalid --entity. Allowed values: workshops, competitions");
            return 1;
        }

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
            if (isWorkshops)
            {
                logger.LogInformation("Converting workshops to drafts...");

                var workshops = GetWorkshopsWithRelatedData(dbContext);

                var workshopDrafts = await workshops
                    .Where(w => !dbContext.WorkshopDrafts.Any(wd => wd.WorkshopId == w.Id) && w.CreatedAt > cutoffUtc)
                    .AsAsyncEnumerable()
                    .Select(ConvertWorkshopToDraft)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Found {Count} workshops to convert", workshopDrafts.Count);

                await dbContext.WorkshopDrafts.AddRangeAsync(workshopDrafts, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                logger.LogInformation("Transaction committed successfully. Converted {Count} workshops to drafts", workshopDrafts.Count);
            }
            else if (isCompetitions)
            {
                logger.LogInformation("Converting competitive events to drafts...");

                var eventsQuery = GetCompetitiveEventsWithRelatedData(dbContext);

                var competitiveEventDrafts = await eventsQuery
                    .Where(e => !dbContext.CompetitiveEventDrafts.Any(ced => ced.CompetitiveEventId == e.Id) && e.CreatedAt > cutoffUtc)
                    .AsAsyncEnumerable()
                    .Select(ConvertCompetitiveEventToDraft)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Found {Count} competitive events to convert", competitiveEventDrafts.Count);

                await dbContext.CompetitiveEventDrafts.AddRangeAsync(competitiveEventDrafts, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                logger.LogInformation("Transaction committed successfully. Converted {Count} competitive events to drafts", competitiveEventDrafts.Count);
            }
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

    private static IQueryable<CompetitiveEvent> GetCompetitiveEventsWithRelatedData(OutOfSchoolDbContext dbContext)
    {
        return dbContext.CompetitiveEvents
            .Include(e => e.SubDirections)
            .ThenInclude(sd => sd.Direction)
            .Include(e => e.CompetitiveEventDescriptionItems)
            .Include(e => e.Coverage)
            .IncludeContactsWithCodeficatorHierarchy()
            .Include(e => e.Images)
            .Include(e => e.OrganizerOfTheEvent)
            .Where(e => !e.IsDeleted);
    }

    private static CompetitiveEventDraft ConvertCompetitiveEventToDraft(CompetitiveEvent competitiveEvent)
    {
        var dto = competitiveEvent.ToV2Dto();

        var draft = dto.ToDraft();

        draft.DraftStatus = CompetitiveEventDraftStatus.PendingModeration;
        draft.RejectionMessage = null;

        if (competitiveEvent.Images != null && competitiveEvent.Images.Count != 0)
        {
            draft.Images = competitiveEvent.Images
                .Select(originalImage => new Image<CompetitiveEventDraft>
                {
                    EntityId = draft.Id,
                    ExternalStorageId = originalImage.ExternalStorageId
                })
                .ToList();
        }
        else
        {
            draft.Images = [];
        }

        return draft;
    }
}
