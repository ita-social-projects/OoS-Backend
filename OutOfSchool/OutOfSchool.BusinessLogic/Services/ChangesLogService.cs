using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Changes;
using OutOfSchool.BusinessLogic.Services.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using System.Linq.Expressions;

namespace OutOfSchool.BusinessLogic.Services;

public class ChangesLogService(
    IOptions<ChangesLogConfig> config,
    IChangesLogRepository changesLogRepository,
    IProviderRepository providerRepository,
    IApplicationRepository applicationRepository,
    IWorkshopDraftRepository workshopDraftRepository,
    IEntityRepository<long, EmployeeChangesLog> employeeChangesLogRepository,
    IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository,
    IWorkshopRepository workshopRepository,
    ILogger<ChangesLogService> logger,
    IValueProjector valueProjector,
    ICurrentUserService currentUserService,
    IMinistryAdminService ministryAdminService,
    IRegionAdminService regionAdminService,
    IAreaAdminService areaAdminService,
    ICodeficatorService codeficatorService,
    INestedObjectChangeLogger nestedObjectChangeLogger,
    ICollectionChangeLogger collectionChangeLogger) : IChangesLogService
{
    public const char WORD_SEPARATOR_SPACE = ' ';
    public const char WORD_SEPARATOR_COMMA = ',';

    private const string WorkshopDraftEntityName = nameof(WorkshopDraft);

    private static readonly char[] wordSplitSymbols = [ WORD_SEPARATOR_SPACE, WORD_SEPARATOR_COMMA ];


    public int AddEntityChangesToDbContext<TEntity>(TEntity entity, string userId)
        where TEntity : class, IKeyedEntity, new()
    {
        if (!IsLoggingAllowed<TEntity>(out var trackedProperties))
        {
            logger.LogDebug($"Logging is not allowed for the '{typeof(TEntity).Name}' entity type.");

            return 0;
        }

        logger.LogDebug($"Logging of the '{typeof(TEntity).Name}' entity changes started.");

        var result = changesLogRepository.AddChangesLogToDbContext(entity, userId, trackedProperties, valueProjector.ProjectValue);

        logger.LogDebug($"Added {result.Count} records to the Changes Log.");

        return result.Count;
    }

    public async Task<bool> AddCreatingOfEntityToDbContext<TEntity>(TEntity entity, string userId)
        where TEntity : class, IKeyedEntity, new()
    {
        if (!IsLoggingAllowed<TEntity>())
        {
            logger.LogDebug($"Logging is not allowed for the '{typeof(TEntity).Name}' entity type.");

            return false;
        }

        logger.LogDebug($"Logging of the '{typeof(TEntity).Name}' entity creating started.");

        var result = await changesLogRepository.AddCreatingOfEntityToChangesLog(entity, userId).ConfigureAwait(false);

        logger.LogDebug($"Added record to the Changes Log.");

        return result is not null;
    }

    public async Task<SearchResult<ProviderChangesLogDto>> GetProviderChangesLogAsync(ProviderChangesLogRequest request)
    {
        var changeLogFilter = request.ToFilter();

        var predicate = PredicateBuilder.True<Provider>();

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(p => p.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(p => p.InstitutionId == regionAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

            if (subSettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<Provider>();

                foreach (var item in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(x => x.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == item));
                }

                predicate = predicate.And(tempPredicate);
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(p => p.InstitutionId == areaAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

            predicate = predicate.And(x => x.Contacts.Any(c => c.IsDefault && subSettlementsIds.Contains(c.Address.CATOTTGId)));
        }

        var changesLog = await GetChangesLogAsync(changeLogFilter).ConfigureAwait(false);

        var providers = providerRepository.Get(whereExpression: predicate);

        var query = changesLog
                .Join(
                    providers,
                    l => l.EntityIdGuid,
                    p => p.Id,
                    (l, provider) => l.ToDto(provider)
                )
                .IgnoreQueryFilters();

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<ProviderChangesLogDto>
        {
            Entities = entities,
            TotalAmount = query.Count(),
        };
    }

    public async Task<SearchResult<ApplicationChangesLogDto>> GetApplicationChangesLogAsync(ApplicationChangesLogRequest request)
    {
        var changeLogFilter = request.ToFilter();

        var predicate = PredicateBuilder.True<Application>();

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(a => a.Workshop.Provider.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(a => a.Workshop.Provider.InstitutionId == regionAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

            if (subSettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<Application>();

                foreach (var item in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(a => a.Workshop.Provider.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == item));
                }

                predicate = predicate.And(tempPredicate);
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(a => a.Workshop.Provider.InstitutionId == areaAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

            predicate = predicate.And(a => a.Workshop.Provider.Contacts.Any(c => c.IsDefault && subSettlementsIds.Contains(c.Address.CATOTTGId)));
        }

        var changesLog = await GetChangesLogAsync(changeLogFilter).ConfigureAwait(false);

        var applications = applicationRepository.Get(whereExpression: predicate);

        var query = changesLog
                .Join(
                    applications,
                    l => l.EntityIdGuid,
                    a => a.Id,
                    (l, app) => l.ToDto(app)
                )
                .IgnoreQueryFilters();

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<ApplicationChangesLogDto>
        {
            Entities = entities,
            TotalAmount = query.Count(),
        };
    }

    public async Task<SearchResult<EmployeeChangesLogDto>> GetEmployeeChangesLogAsync(EmployeeChangesLogRequest request)
    {
        ValidateFilter(request);

        var where = GetQueryFilter(request);
        var sortExpression = this.GetEmployeeChangesOrderParams();

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            where = where.And(p => p.Provider.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            where = where.And(p => p.Provider.InstitutionId == regionAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

            if (subSettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<EmployeeChangesLog>();

                foreach (var item in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(x => x.Provider.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == item));
                }

                where = where.And(tempPredicate);
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
            where = where.And(p => p.Provider.InstitutionId == areaAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

            where = where.And(a => a.Provider.Contacts.Any(c => c.IsDefault && subSettlementsIds.Contains(c.Address.CATOTTGId)));
        }

        var count = await employeeChangesLogRepository.Count(where).ConfigureAwait(false);
        var query = employeeChangesLogRepository
            .Get(skip: request.From, take: request.Size, whereExpression: where, orderBy: sortExpression)
            .AsNoTracking()
            .Select(x => x.ToDto())
            .IgnoreQueryFilters();

        var entities = await query.ToListAsync().ConfigureAwait(false);

        return new SearchResult<EmployeeChangesLogDto>
        {
            Entities = entities,
            TotalAmount = count,
        };
    }

    public async Task<SearchResult<ParentBlockedByAdminChangesLogDto>> GetParentBlockedByAdminChangesLogAsync(
    ParentBlockedByAdminChangesLogRequest request)
    {
        ValidateFilter(request);
        var where = GetQueryFilter(request);
        var sortExpression = GetParentBlockedByAdminChangesOrderParams();
        var count = await parentBlockedByAdminLogRepository.Count(where).ConfigureAwait(false);
        var query = parentBlockedByAdminLogRepository
            .Get(skip: request.From, take: request.Size, whereExpression: where, orderBy: sortExpression)
            .AsNoTracking()
            .Select(x => x.ToDto())
            .IgnoreQueryFilters();

        var entities = await query.ToListAsync().ConfigureAwait(false);

        return new SearchResult<ParentBlockedByAdminChangesLogDto>
        {
            Entities = entities,
            TotalAmount = count,
        };
    }
    
    /// <inheritdoc />
    public async Task<SearchResult<WorkshopChangesLogDto>> GetWorkshopChangesLogAsync(WorkshopChangesLogRequest request)
    {
        ValidateFilter(request);
        var filter = new ChangesLogFilter
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            EntityId = request.EntityId,
            EntityType = "Workshop",
            From = request.From,
            Size = request.Size,
            PropertyName = request.PropertyName,
            SearchString = request.SearchString
        };

        var changesLog = await GetChangesLogAsync(filter).ConfigureAwait(false);

        var workshops = workshopRepository.Get();

        var query = changesLog
            .Join(workshops,
                l => l.EntityIdGuid,
                w => w.Id,
                (l, workshop) => new WorkshopChangesLogDto
                {
                    FieldName = l.PropertyName,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue,
                    UpdatedDate = l.UpdatedDate,
                    User = l.User.ToShortUser(),
                    WorkshopId = workshop.Id
                });

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<WorkshopChangesLogDto>
        {
            Entities = entities,
            TotalAmount = await query.CountAsync(),
        };
    }

    /// <inheritdoc />
    public async Task<SearchResult<WorkshopDraftChangesLogDto>> GetWorkshopDraftChangesLogAsync(WorkshopDraftChangesLogRequest request)
    {
        var changeLogFilter = request.ToFilter();

        ValidateFilter(changeLogFilter);

        var predicate = await GetWorkshopDraftAccessPredicateAsync();

        var changesLog = await GetChangesLogAsync(changeLogFilter).ConfigureAwait(false);

        var drafts = workshopDraftRepository.Get(whereExpression: predicate).IgnoreQueryFilters();

        var query = changesLog
                .Join(
                    drafts,
                    l => l.EntityIdGuid,
                    d => d.Id,
                    (l, draft) => l.ToDto(draft)
                )
                .IgnoreQueryFilters();

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<WorkshopDraftChangesLogDto>
        {
            Entities = entities,
            TotalAmount = await query.CountAsync(),
        };
    }

    /// <inheritdoc />
    public void LogWorkshopDraftChanges(
        WorkshopDraftContent oldContent,
        WorkshopDraftContent newContent,
        Guid draftId,
        string userId)
    {
        if (!IsLoggingAllowed<WorkshopDraftContent>(out var trackedProperties))
        {
            logger.LogDebug("Logging is not allowed for WorkshopDraftContent.");
            return;
        }

        var trackedPropertiesSet = trackedProperties.ToHashSet();

        var contactCollectionProperties = new[]
{
            "Contacts.Phones",
            "Contacts.Emails",
            "Contacts.SocialNetworks"
        };

        // Combine all collection properties
        var collectionProperties = new[]
        {
            nameof(WorkshopDraftContent.WorkshopDescriptionItems)
        }.Concat(contactCollectionProperties).ToArray();

        var nonCollectionProperties = trackedProperties.Except(collectionProperties).ToList();

        // Log regular properties
        var nestedLogs = nestedObjectChangeLogger.CompareAndLogChanges(
            oldContent,
            newContent,
            draftId,
            WorkshopDraftEntityName,
            userId,
            nonCollectionProperties,
            valueProjector);

        // Log collections
        var collectionLogs = new List<ChangesLog>();

        // Log WorkshopDescriptionItems
        if (trackedPropertiesSet.Contains(nameof(WorkshopDraftContent.WorkshopDescriptionItems)))
        {
            var descriptionItemsLogs = collectionChangeLogger.CompareCollections(
                oldContent.WorkshopDescriptionItems ?? [],
                newContent.WorkshopDescriptionItems ?? [],
                item => item.SectionName,
                draftId,
                WorkshopDraftEntityName,
                userId,
                nameof(WorkshopDraftContent.WorkshopDescriptionItems),
                valueProjector,
                true);

            collectionLogs.AddRange(descriptionItemsLogs);
        }

        // Log contact collections
        var trackedContactProperties = contactCollectionProperties
            .Where(trackedPropertiesSet.Contains)
            .ToArray();

        if (trackedContactProperties.Length > 0)
        {
            var contactsLogs = LogContactCollections(
                oldContent.Contacts ?? [],
                newContent.Contacts ?? [],
                trackedContactProperties,
                draftId,
                userId);

            collectionLogs.AddRange(contactsLogs);
        }

        // Save combined logs
        var allLogs = nestedLogs.Concat(collectionLogs).ToList();

        if (allLogs.Count > 0)
        {
            changesLogRepository.AddChangeLogsToDbContext(allLogs);
            logger.LogInformation("Logged {Count} changes for WorkshopDraft {DraftId}.", allLogs.Count, draftId);
        }
    }

    /// <summary>
    /// Logs changes in contact collections (phones, emails, social networks) for all contacts.
    /// </summary>
    private List<ChangesLog> LogContactCollections(
        List<Contacts> oldContacts,
        List<Contacts> newContacts,
        string[] trackedCollectionProperties,
        Guid draftId,
        string userId)
    {
        var logs = new List<ChangesLog>();
        var maxCount = Math.Max(oldContacts.Count, newContacts.Count);

        for (int i = 0; i < maxCount; i++)
        {
            var oldContact = i < oldContacts.Count ? oldContacts[i] : null;
            var newContact = i < newContacts.Count ? newContacts[i] : null;

            if (oldContact == null && newContact == null)
                continue;

            var contactIdentifier = GetContactIdentifier(oldContact, newContact, i);

            foreach (var trackedProperty in trackedCollectionProperties)
            {
                var collectionType = GetContactCollectionType(trackedProperty);

                if (!collectionType.HasValue)
                    continue;

                var propertyPrefix = $"Contacts[{contactIdentifier}].{collectionType}";

                var collectionLogs = GetContactCollectionLogs(
                    oldContact,
                    newContact,
                    collectionType.Value,
                    draftId,
                    userId,
                    propertyPrefix);

                logs.AddRange(collectionLogs);
            }
        }

        return logs;
    }

    private static ContactCollectionType? GetContactCollectionType(string trackedProperty) =>
        trackedProperty switch
        {
            "Contacts.Phones" => ContactCollectionType.Phones,
            "Contacts.Emails" => ContactCollectionType.Emails,
            "Contacts.SocialNetworks" => ContactCollectionType.SocialNetworks,
            _ => null
        };

    private static string GetContactIdentifier(Contacts oldContact, Contacts newContact, int index)
    {
        var title = newContact?.Title ?? oldContact?.Title;
        return !string.IsNullOrWhiteSpace(title) ? title : $"Index{index}";
    }

    private List<ChangesLog> GetContactCollectionLogs(
       Contacts oldContact,
       Contacts newContact,
       ContactCollectionType collectionType,
       Guid draftId,
       string userId,
       string propertyPrefix) => collectionType switch
       {
           ContactCollectionType.Phones => collectionChangeLogger.CompareCollections(
                oldContact?.Phones ?? [],
                newContact?.Phones ?? [],
                phone => phone.Number,
                draftId,
                WorkshopDraftEntityName,
                userId,
                propertyPrefix,
                valueProjector,
                true),

           ContactCollectionType.Emails => collectionChangeLogger.CompareCollections(
                oldContact?.Emails ?? [],
                newContact?.Emails ?? [],
                email => email.Address,
                draftId,
                WorkshopDraftEntityName,
                userId,
                propertyPrefix,
                valueProjector,
                true),

           ContactCollectionType.SocialNetworks => collectionChangeLogger.CompareCollections(
                oldContact?.SocialNetworks ?? [],
                newContact?.SocialNetworks ?? [],
                sn => $"{sn.Type}_{sn.Url}",
                draftId,
                WorkshopDraftEntityName,
                userId,
                propertyPrefix,
                valueProjector,
                true),

           _ => []
       };

    /// <inheritdoc />
    public void LogImageDeletions(
        IEnumerable<string> oldImageIds,
        IEnumerable<string> newImageIds,
        Guid entityId,
        string entityType,
        string userId)
    {
        var removedImageIds = oldImageIds.Except(newImageIds).ToList();

        if (removedImageIds.Count != 0)
        {
            var logs = removedImageIds.Select(id => new ChangesLog
            {
                EntityType = entityType,
                EntityIdGuid = entityId,
                PropertyName = $"Images.Removed.ExternalStorageId",
                OldValue = id,
                NewValue = null,
                UserId = userId,
                UpdatedDate = DateTime.UtcNow,
            }).ToList();

            changesLogRepository.AddChangeLogsToDbContext(logs);

            logger.LogInformation("Logged {Count} image deletions for {EntityType} {EntityId}.",
                logs.Count, entityType, entityId);
        }
        else
        {
            logger.LogDebug("No image deletions detected for {EntityType} {EntityId}.",
                entityType, entityId);
        }
    }

    private async Task<IQueryable<ChangesLog>> GetChangesLogAsync(ChangesLogFilter filter)
    {
        ValidateFilter(filter);

        var where = GetQueryFilter(filter);
        var sortExpression = GetOrderParams();

        var query = changesLogRepository.Get(
                skip: filter.From, 
                take: filter.Size, 
                whereExpression: where, 
                orderBy: sortExpression)
            .AsNoTracking();

        return query;
    }

    private bool IsLoggingAllowed<TEntity>(out string[] trackedProperties)
        => config.Value.TrackedProperties.TryGetValue(typeof(TEntity).Name, out trackedProperties);

    private bool IsLoggingAllowed<TEntity>()
        => config.Value.TrackedProperties.ContainsKey(typeof(TEntity).Name);

    private Expression<Func<ChangesLog, bool>> GetQueryFilter(ChangesLogFilter filter)
    {
        Expression<Func<ChangesLog, bool>> expr = x => x.EntityType == filter.EntityType;

        if (filter.PropertyName != null)
        {
            expr = expr.And(x => x.PropertyName == filter.PropertyName);
        }

        if (filter.EntityId != null)
        {
            if (Guid.TryParse(filter.EntityId, out var recordIdGuid))
            {
                expr = expr.And(x => x.EntityIdGuid == recordIdGuid);
            }
            else if (long.TryParse(filter.EntityId, out var recordIdLong))
            {
                expr = expr.And(x => x.EntityIdLong == recordIdLong);
            }
        }

        if (filter.DateFrom.HasValue)
        {
            expr = expr.And(x => x.UpdatedDate >= filter.DateFrom.Value.Date);
        }

        if (filter.DateTo.HasValue)
        {
            expr = expr.And(x => x.UpdatedDate < filter.DateTo.Value.NextDayStart());
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempExpr = PredicateBuilder.False<ChangesLog>();

            foreach (var word in filter.SearchString.Split(wordSplitSymbols, StringSplitOptions.RemoveEmptyEntries))
            {
                tempExpr = tempExpr.Or(
                    x => x.User.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.OldValue.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.NewValue.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            expr = expr.And(tempExpr);
        }

        return expr;
    }

    private Expression<Func<EmployeeChangesLog, bool>> GetQueryFilter(EmployeeChangesLogRequest request)
    {
        var expr = PredicateBuilder.True<EmployeeChangesLog>();
        if (request.OperationType != null)
        {
            expr = expr.And(x => x.OperationType == request.OperationType);
        }

        if (request.DateFrom.HasValue)
        {
            expr = expr.And(x => x.OperationDate >= request.DateFrom.Value.Date);
        }

        if (request.DateTo.HasValue)
        {
            expr = expr.And(x => x.OperationDate < request.DateTo.Value.NextDayStart());
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var tempExpr = PredicateBuilder.False<EmployeeChangesLog>();

            foreach (var word in request.SearchString.Split(wordSplitSymbols, StringSplitOptions.RemoveEmptyEntries))
            {
                tempExpr = tempExpr.Or(
                    x => x.User.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.Provider.Contacts.Any(c => c.IsDefault && c.Address.CATOTTG.Name.Contains(word, StringComparison.InvariantCulture)));
            }

            expr = expr.And(tempExpr);
        }

        return expr;
    }

    private Expression<Func<ParentBlockedByAdminLog, bool>> GetQueryFilter(ParentBlockedByAdminChangesLogRequest request)
    {
        var expr = PredicateBuilder.True<ParentBlockedByAdminLog>();

        expr = request.ShowParents switch
        {
            ShowParents.All => expr,
            ShowParents.Blocked => expr.And(x => x.IsBlocked),
            ShowParents.Unblocked => expr.And(x => !x.IsBlocked),
            _ => throw new NotImplementedException(),
        };

        if (request.DateFrom.HasValue)
        {
            expr = expr.And(x => x.OperationDate >= request.DateFrom.Value.Date);
        }

        if (request.DateTo.HasValue)
        {
            expr = expr.And(x => x.OperationDate < request.DateTo.Value.NextDayStart());
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var tempExpr = PredicateBuilder.False<ParentBlockedByAdminLog>();

            foreach (var word in request.SearchString.Split(wordSplitSymbols, StringSplitOptions.RemoveEmptyEntries))
            {
                tempExpr = tempExpr.Or(
                    x => x.Parent.User.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.Parent.User.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.Parent.User.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.Reason.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            expr = expr.And(tempExpr);
        }

        return expr;
    }

    private Dictionary<Expression<Func<ChangesLog, dynamic>>, SortDirection> GetOrderParams()
    {
        // Returns default ordering so far...
        var sortExpression = new Dictionary<Expression<Func<ChangesLog, object>>, SortDirection>
        {
            { x => x.UpdatedDate, SortDirection.Descending },
        };

        return sortExpression;
    }

    private Dictionary<Expression<Func<EmployeeChangesLog, dynamic>>, SortDirection> GetEmployeeChangesOrderParams()
    {
        // Returns default ordering so far...
        var sortExpression = new Dictionary<Expression<Func<EmployeeChangesLog, object>>, SortDirection>
        {
            { x => x.OperationDate, SortDirection.Descending },
        };

        return sortExpression;
    }

    private Dictionary<Expression<Func<ParentBlockedByAdminLog, dynamic>>, SortDirection> GetParentBlockedByAdminChangesOrderParams()
    {
        var sortExpression = new Dictionary<Expression<Func<ParentBlockedByAdminLog, object>>, SortDirection>
        {
            { x => x.OperationDate, SortDirection.Descending },
        };

        return sortExpression;
    }

    private void ValidateFilter(OffsetFilter filter)
    {
        ModelValidationHelper.ValidateOffsetFilter(filter);
    }

    /// <summary>
    /// Constructs a dynamic predicate to limit access to workshop drafts
    /// based on the role and region of the currently authenticated user.
    /// </summary>
    /// <returns>An expression used to filter workshop drafts for the current admin user.</returns>
    private async Task<Expression<Func<WorkshopDraft, bool>>> GetWorkshopDraftAccessPredicateAsync()
    {
        var predicate = PredicateBuilder.True<WorkshopDraft>();

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(d => d.Provider.InstitutionId == ministryAdmin.InstitutionId);
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(d => d.Provider.InstitutionId == regionAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId).ConfigureAwait(false);

            if (subSettlementsIds.Any())
            {
                var tempPredicate = PredicateBuilder.False<WorkshopDraft>();
                foreach (var id in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(d => d.Provider.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == id));
                }
                predicate = predicate.And(tempPredicate);
            }
        }

        if (currentUserService.IsAreaAdmin())
        {
            var areaAdmin = await areaAdminService.GetByUserId(currentUserService.UserId);
            predicate = predicate.And(d => d.Provider.InstitutionId == areaAdmin.InstitutionId);

            var subSettlementsIds = await codeficatorService
                .GetAllChildrenIdsByParentIdAsync(areaAdmin.CATOTTGId).ConfigureAwait(false);

            predicate = predicate.And(d => d.Provider.Contacts.Any(c => c.IsDefault && subSettlementsIds.Contains(c.Address.CATOTTGId)));
        }

        return predicate;
    }
}