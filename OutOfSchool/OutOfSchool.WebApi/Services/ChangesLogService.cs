﻿using System.Data;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.Extensions.Options;
using Nest;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services;

public class ChangesLogService : IChangesLogService
{
    private readonly IOptions<ChangesLogConfig> config;
    private readonly IChangesLogRepository changesLogRepository;
    private readonly IProviderRepository providerRepository;
    private readonly IApplicationRepository applicationRepository;
    private readonly IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository;
    private readonly ILogger<ChangesLogService> logger;
    private readonly IMapper mapper;
    private readonly IValueProjector valueProjector;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;

    public ChangesLogService(
        IOptions<ChangesLogConfig> config,
        IChangesLogRepository changesLogRepository,
        IProviderRepository providerRepository,
        IApplicationRepository applicationRepository,
        IEntityRepository<long, ProviderAdminChangesLog> providerAdminChangesLogRepository,
        ILogger<ChangesLogService> logger,
        IMapper mapper,
        IValueProjector valueProjector,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService)
    {
        this.config = config;
        this.changesLogRepository = changesLogRepository;
        this.providerRepository = providerRepository;
        this.applicationRepository = applicationRepository;
        this.providerAdminChangesLogRepository = providerAdminChangesLogRepository;
        this.logger = logger;
        this.mapper = mapper;
        this.valueProjector = valueProjector;
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
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

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            changeLogFilter.InstitutionId = ministryAdmin.InstitutionId;
        }

        var (changesLog, count) = await GetChangesLogAsync(changeLogFilter)
            .ConfigureAwait(false);
        var providers = providerRepository.Get();

        var query = from l in changesLog
                    join p in providers
                        on l.EntityIdGuid equals p.Id into pg
                    from provider in pg.DefaultIfEmpty()
                    where provider.InstitutionId == (changeLogFilter.InstitutionId ?? provider.InstitutionId)
                    select new ProviderChangesLogDto
                    {
                        FieldName = l.PropertyName,
                        OldValue = l.OldValue,
                        NewValue = l.NewValue,
                        UpdatedDate = l.UpdatedDate,
                        User = mapper.Map<ShortUserDto>(l.User),
                        ProviderId = l.EntityIdGuid.Value,
                        ProviderTitle = provider == null ? null : provider.FullTitle,
                        ProviderCity = provider == null || provider.LegalAddress == null
                            ? null : provider.LegalAddress.CATOTTG.Name,
                        InstitutionTitle = provider == null || provider.Institution == null
                            ? null : provider.Institution.Title,
                    };

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<ProviderChangesLogDto>
        {
            Entities = entities,
            TotalAmount = currentUserService.IsMinistryAdmin() ? query.Count() : count,
        };
    }

    public async Task<SearchResult<ApplicationChangesLogDto>> GetApplicationChangesLogAsync(ApplicationChangesLogRequest request)
    {
        var changeLogFilter = mapper.Map<ChangesLogFilter>(request);

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            changeLogFilter.InstitutionId = ministryAdmin.InstitutionId;
        }

        var (changesLog, count) = await GetChangesLogAsync(changeLogFilter)
            .ConfigureAwait(false);
        var applications = applicationRepository.Get();

        var query = from l in changesLog
                    join a in applications
                        on l.EntityIdGuid equals a.Id into ag
                    from app in ag.DefaultIfEmpty()
                    where app.Workshop.Provider.InstitutionId == (changeLogFilter.InstitutionId ?? app.Workshop.Provider.InstitutionId)
                    select new ApplicationChangesLogDto
                    {
                        FieldName = l.PropertyName,
                        OldValue = l.OldValue,
                        NewValue = l.NewValue,
                        UpdatedDate = l.UpdatedDate,
                        User = mapper.Map<ShortUserDto>(l.User),
                        ApplicationId = l.EntityIdGuid.Value,
                        WorkshopTitle = app == null ? null : app.Workshop.Title,
                        WorkshopCity = app == null ? null : app.Workshop.Address.CATOTTG.Name,
                        ProviderTitle = app == null ? null : app.Workshop.ProviderTitle,
                        InstitutionTitle = app == null || app.Workshop.Provider.Institution == null
                            ? null : app.Workshop.Provider.Institution.Title,
                    };

        var entities = await query.Skip(request.From).Take(request.Size).ToListAsync().ConfigureAwait(false);

        return new SearchResult<ApplicationChangesLogDto>
        {
            Entities = entities,
            TotalAmount = currentUserService.IsMinistryAdmin() ? query.Count() : count,
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
            where = where.And(x => x.Provider.InstitutionId == ministryAdmin.InstitutionId);
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
                WorkshopCity = x.ManagedWorkshop.Address.CATOTTG.Name,
                OperationType = x.OperationType,
                OperationDate = x.OperationDate,
                User = mapper.Map<ShortUserDto>(x.User),
                InstitutionTitle = x.Provider.Institution == null
                    ? null : x.Provider.Institution.Title,
            }).IgnoreQueryFilters();

        var entities = await query.ToListAsync().ConfigureAwait(false);

        return new SearchResult<ProviderAdminChangesLogDto>
        {
            Entities = entities,
            TotalAmount = count,
        };
    }

    private async Task<(IQueryable<ChangesLog>, int)> GetChangesLogAsync(ChangesLogFilter filter)
    {
        ValidateFilter(filter);

        var where = GetQueryFilter(filter);
        var sortExpression = GetOrderParams();

        var count = await changesLogRepository.Count(where).ConfigureAwait(false);

        var query = changesLogRepository.Get(filter.From, filter.Size, string.Empty, where, sortExpression, true);

        return (query, count);
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
            expr = expr.And(x => x.UpdatedDate < filter.DateTo.Value.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempExpr = PredicateBuilder.False<ChangesLog>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
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
            expr = expr.And(x => x.OperationDate < request.DateTo.Value.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchString))
        {
            var tempExpr = PredicateBuilder.False<ProviderAdminChangesLog>();

            foreach (var word in request.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
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

    private void ValidateFilter(OffsetFilter filter)
    {
        ModelValidationHelper.ValidateOffsetFilter(filter);
    }
}