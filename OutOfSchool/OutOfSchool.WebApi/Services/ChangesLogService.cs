﻿using System.Data;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Changes;

namespace OutOfSchool.WebApi.Services;

public class ChangesLogService : IChangesLogService
{
    public const char WORD_SEPARATOR_SPACE = ' ';
    public const char WORD_SEPARATOR_COMMA = ',';

    private static char[] wordSplitSymbols = new char[] { WORD_SEPARATOR_SPACE, WORD_SEPARATOR_COMMA };

    private readonly IOptions<ChangesLogConfig> config;
    private readonly IChangesLogRepository changesLogRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IApplicationRepository applicationRepository;
    private readonly IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository;
    private readonly IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository;
    private readonly ILogger<ChangesLogService> logger;
    private readonly IMapper mapper;
    private readonly IValueProjector valueProjector;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly IAreaAdminService areaAdminService;
    private readonly ICodeficatorService codeficatorService;

    public ChangesLogService(
        IOptions<ChangesLogConfig> config,
        IChangesLogRepository changesLogRepository,
        IProviderRepository providerRepository,
        IApplicationRepository applicationRepository,
        IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository,
        IEntityAddOnlyRepository<long, ParentBlockedByAdminLog> parentBlockedByAdminLogRepository,
        ILogger<ChangesLogService> logger,
        IMapper mapper,
        IValueProjector valueProjector,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        IAreaAdminService areaAdminService,
        ICodeficatorService codeficatorService)
    {
        this.config = config;
        this.changesLogRepository = changesLogRepository;
        this.providerRepository = providerRepository;
        this.applicationRepository = applicationRepository;
        this.providerAdminChangesLogRepository = providerAdminChangesLogRepository;
        this.parentBlockedByAdminLogRepository = parentBlockedByAdminLogRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.valueProjector = valueProjector;
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
        this.areaAdminService = areaAdminService ?? throw new ArgumentNullException(nameof(areaAdminService));
        this.codeficatorService = codeficatorService ?? throw new ArgumentNullException(nameof(codeficatorService));
    }

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

    public async Task<SearchResult<ProviderChangesLogDto>> GetProviderChangesLogAsync(ProviderChangesLogRequest request)
    {
        var changeLogFilter = mapper.Map<ChangesLogFilter>(request);

        Expression<Func<Provider, bool>> predicate = PredicateBuilder.True<Provider>();

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
                    tempPredicate = tempPredicate.Or(x => x.LegalAddress.CATOTTGId == item);
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

            predicate = predicate.And(x => subSettlementsIds.Contains(x.LegalAddress.CATOTTGId));
        }

        var changesLog = await GetChangesLogAsync(changeLogFilter).ConfigureAwait(false);

        var providers = providerRepository.Get(whereExpression: predicate);

        var query = changesLog
                .Join(
                    providers,
                    l => l.EntityIdGuid,
                    p => p.Id,
                    (l, provider) => new ProviderChangesLogDto
                    {
                        FieldName = l.PropertyName,
                        OldValue = l.OldValue,
                        NewValue = l.NewValue,
                        UpdatedDate = l.UpdatedDate,
                        User = mapper.Map<ShortUserDto>(l.User),
                        ProviderId = l.EntityIdGuid.Value,
                        ProviderTitle = provider.FullTitle,
                        ProviderCity = provider.LegalAddress.CATOTTG.Name,
                        InstitutionTitle = provider.Institution.Title,
                    })
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
        var changeLogFilter = mapper.Map<ChangesLogFilter>(request);

        Expression<Func<Application, bool>> predicate = PredicateBuilder.True<Application>();

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
                    tempPredicate = tempPredicate.Or(a => a.Workshop.Provider.LegalAddress.CATOTTGId == item);
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

            predicate = predicate.And(a => subSettlementsIds.Contains(a.Workshop.Provider.LegalAddress.CATOTTGId));
        }

        var changesLog = await GetChangesLogAsync(changeLogFilter).ConfigureAwait(false);

        var applications = applicationRepository.Get(whereExpression: predicate);

        var query = changesLog
                .Join(
                    applications,
                    l => l.EntityIdGuid,
                    a => a.Id,
                    (l, app) => new ApplicationChangesLogDto
                    {
                        FieldName = l.PropertyName,
                        OldValue = l.OldValue,
                        NewValue = l.NewValue,
                        UpdatedDate = l.UpdatedDate,
                        User = mapper.Map<ShortUserDto>(l.User),
                        ApplicationId = l.EntityIdGuid.Value,
                        WorkshopTitle = app.Workshop.Title,
                        WorkshopCity = app.Workshop.Address.CATOTTG.Name,
                        ProviderTitle = app.Workshop.ProviderTitle,
                        InstitutionTitle = app.Workshop.Provider.Institution.Title,
                    })
                .IgnoreQueryFilters();

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<ApplicationChangesLogDto>
        {
            Entities = entities,
            TotalAmount = query.Count(),
        };
    }

    public async Task<SearchResult<ProviderAdminChangesLogDto>> GetProviderAdminChangesLogAsync(ProviderAdminChangesLogRequest request)
    {
        ValidateFilter(request);

        var where = GetQueryFilter(request);
        var sortExpression = GetProviderAdminChangesOrderParams();

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
                var tempPredicate = PredicateBuilder.False<ProviderAdminChangesLog>();

                foreach (var item in subSettlementsIds)
                {
                    tempPredicate = tempPredicate.Or(x => x.Provider.LegalAddress.CATOTTGId == item);
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

            where = where.And(a => subSettlementsIds.Contains(a.Provider.LegalAddress.CATOTTGId));
        }

        var count = await providerAdminChangesLogRepository.Count(where).ConfigureAwait(false);
        var query = providerAdminChangesLogRepository
            .Get(request.From, request.Size, string.Empty, where, sortExpression, true)
            .Select(x => new ProviderAdminChangesLogDto()
            {
                ProviderAdminId = x.ProviderAdminUserId,
                ProviderAdminFullName = $"{x.ProviderAdminUser.LastName} {x.ProviderAdminUser.FirstName} {x.ProviderAdminUser.MiddleName}".TrimEnd(),
                ProviderTitle = x.Provider.FullTitle,
                WorkshopTitle = x.ManagedWorkshop.Title,
                WorkshopCity = x.Provider.LegalAddress.CATOTTG.Name,
                OperationType = x.OperationType,
                OperationDate = x.OperationDate,
                User = mapper.Map<ShortUserDto>(x.User),
                InstitutionTitle = x.Provider.Institution == null
                    ? null : x.Provider.Institution.Title,
                PropertyName = x.PropertyName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
            })
            .IgnoreQueryFilters();

        var entities = await query.ToListAsync().ConfigureAwait(false);

        return new SearchResult<ProviderAdminChangesLogDto>
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
            .Get(request.From, request.Size, string.Empty, where, sortExpression, true)
            .Select(x => new ParentBlockedByAdminChangesLogDto()
            {
                ParentId = x.ParentId,
                ParentFullName = $"{x.Parent.User.LastName} {x.Parent.User.FirstName} {x.Parent.User.MiddleName}".TrimEnd(),
                User = mapper.Map<ShortUserDto>(x.User),
                OperationDate = x.OperationDate,
                Reason = x.Reason,
                IsBlocked = x.IsBlocked,
            }).IgnoreQueryFilters();

        var entities = await query.ToListAsync().ConfigureAwait(false);

        return new SearchResult<ParentBlockedByAdminChangesLogDto>
        {
            Entities = entities,
            TotalAmount = count,
        };
    }

    private async Task<IQueryable<ChangesLog>> GetChangesLogAsync(ChangesLogFilter filter)
    {
        ValidateFilter(filter);

        var where = GetQueryFilter(filter);
        var sortExpression = GetOrderParams();

        var query = changesLogRepository.Get(filter.From, filter.Size, string.Empty, where, sortExpression, true);

        return query;
    }

    private bool IsLoggingAllowed<TEntity>(out string[] trackedProperties)
        => config.Value.TrackedProperties.TryGetValue(typeof(TEntity).Name, out trackedProperties);

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

    private Expression<Func<ProviderAdminChangesLog, bool>> GetQueryFilter(ProviderAdminChangesLogRequest request)
    {
        Expression<Func<ProviderAdminChangesLog, bool>> expr = PredicateBuilder.True<ProviderAdminChangesLog>();

        expr = request.AdminType switch
        {
            ProviderAdminType.All => expr,
            ProviderAdminType.Deputies => expr.And(x => x.ManagedWorkshopId == null),
            ProviderAdminType.Assistants => expr.And(x => x.ManagedWorkshopId != null),
            _ => throw new NotImplementedException(),
        };

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
            var tempExpr = PredicateBuilder.False<ProviderAdminChangesLog>();

            foreach (var word in request.SearchString.Split(wordSplitSymbols, StringSplitOptions.RemoveEmptyEntries))
            {
                tempExpr = tempExpr.Or(
                    x => x.User.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.User.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.ManagedWorkshop.Address.CATOTTG.Name.Contains(word, StringComparison.InvariantCulture));
            }

            expr = expr.And(tempExpr);
        }

        return expr;
    }

    private Expression<Func<ParentBlockedByAdminLog, bool>> GetQueryFilter(ParentBlockedByAdminChangesLogRequest request)
    {
        Expression<Func<ParentBlockedByAdminLog, bool>> expr = PredicateBuilder.True<ParentBlockedByAdminLog>();

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

    private Dictionary<Expression<Func<ProviderAdminChangesLog, dynamic>>, SortDirection> GetProviderAdminChangesOrderParams()
    {
        // Returns default ordering so far...
        var sortExpression = new Dictionary<Expression<Func<ProviderAdminChangesLog, object>>, SortDirection>
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
}