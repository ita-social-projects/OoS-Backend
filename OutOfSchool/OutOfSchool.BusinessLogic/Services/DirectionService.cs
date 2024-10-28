using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Direction entity.
/// </summary>
public class DirectionService : IDirectionService, ISensitiveDirectionService
{
    private readonly IEntityRepositorySoftDeleted<long, Direction> repository;
    private readonly IWorkshopRepository repositoryWorkshop;
    private readonly ILogger<DirectionService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionService"/> class.
    /// </summary>
    /// <param name="repository">Repository for Direction entity.</param>
    /// <param name="repositoryWorkshop">Workshop repository.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="currentUserService">Service for manage current user.</param>
    /// <param name="ministryAdminService">Service for manage ministry admin.</param>
    /// <param name="regionAdminService">Service for managing region admin rigths.</param>
    public DirectionService(
        IEntityRepositorySoftDeleted<long, Direction> repository,
        IWorkshopRepository repositoryWorkshop,
        ILogger<DirectionService> logger,
        IStringLocalizer<SharedResource> localizer,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService)
    {
        this.localizer = localizer;
        this.repository = repository;
        this.repositoryWorkshop = repositoryWorkshop;
        this.logger = logger;
        this.mapper = mapper;
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.ministryAdminService = ministryAdminService ?? throw new ArgumentNullException(nameof(ministryAdminService));
        this.regionAdminService = regionAdminService ?? throw new ArgumentNullException(nameof(regionAdminService));
    }

    /// <inheritdoc/>
    public async Task<DirectionDto> Create(DirectionDto dto)
    {
        logger.LogInformation("Direction creating was started.");

        var direction = mapper.Map<Direction>(dto);

        DirectionValidation(dto);

        var newDirection = await repository.Create(direction).ConfigureAwait(false);

        logger.LogInformation($"Direction with Id = {newDirection?.Id} created successfully.");

        return mapper.Map<DirectionDto>(newDirection);
    }

    /// <inheritdoc/>
    public async Task<Result<DirectionDto>> Delete(long id)
    {
        logger.LogInformation($"Deleting Direction with Id = {id} started.");

        var direction = await repository.GetById(id).ConfigureAwait(false);

        if (direction == null)
        {
            return Result<DirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Direction with Id = {id} is not exists.",
            });
        }

        var workShops = await repositoryWorkshop
            .GetByFilter(w => w.InstitutionHierarchy.Directions.Any(d => d.Id == id))
            .ConfigureAwait(false);

        if (workShops.Any())
        {
            return Result<DirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = localizer["Some workshops assosiated with this direction. Deletion prohibited."],
            });
        }

        try
        {
            await repository.Delete(direction).ConfigureAwait(false);

            logger.LogInformation($"Direction with Id = {id} succesfully deleted.");

            return Result<DirectionDto>.Success(mapper.Map<DirectionDto>(direction));
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Direction with Id = {id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DirectionDto>> GetAll()
    {
        logger.LogInformation("Getting all Directions started.");

        var directions = await repository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!directions.Any()
            ? "Direction table is empty."
            : $"All {directions.Count()} records were successfully received from the Direction table.");

        return directions.OrderBy(x => x.Title).Select(entity => mapper.Map<DirectionDto>(entity)).ToList();
    }

    public async Task<SearchResult<DirectionDto>> GetByFilter(DirectionFilter filter, bool isAdmins)
    {
        logger.LogInformation("Getting Directions by filter started.");

        var (predicate, workshopCountFilter) = await BuildPredicate(filter, isAdmins).ConfigureAwait(false);

        var count = await repository.Count(predicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Direction, object>>, SortDirection>
        {
            { x => x.Title, SortDirection.Ascending },
        };

        var directions = await repository
            .Get(skip: filter.From, take: filter.Size, whereExpression: predicate, orderBy: sortExpression)
            .ToListAsync();

        var workshopCount = await repositoryWorkshop
            .Get(whereExpression: workshopCountFilter
                .And(w => w.InstitutionHierarchy.Directions.Any(d => directions.Contains(d))))
            .SelectMany(w => w.InstitutionHierarchy.Directions)
            .GroupBy(d => d.Id)
            .Select(g => new
            {
                DirectionId = g.Key,
                WorkshopsCount = g.Count() as int?,
            })
            .ToListAsync();

        var directionsWorkshops = (from d in directions
                                   join wc in workshopCount
                                       on d.Id equals wc.DirectionId into dwc
                                   from res in dwc.DefaultIfEmpty()
                                   select mapper.Map<DirectionDto>(d).WithCount(res?.WorkshopsCount ?? 0))
            .ToList();

        logger.LogInformation($"All {directionsWorkshops.Count()} records were successfully received from the Direction table.");

        var result = new SearchResult<DirectionDto>()
        {
            TotalAmount = count,
            Entities = directionsWorkshops,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<DirectionDto> GetById(long id)
    {
        logger.LogInformation($"Getting Direction by Id started. Looking Id = {id}.");

        var direction = await repository.GetById((int)id).ConfigureAwait(false);

        if (direction == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a Direction with Id = {id}.");

        return mapper.Map<DirectionDto>(direction);
    }

    /// <inheritdoc/>
    public async Task<DirectionDto> Update(DirectionDto dto)
    {
        logger.LogInformation($"Updating Direction with Id = {dto?.Id} started.");

        ArgumentNullException.ThrowIfNull(dto);

        var direction = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (direction is null)
        {
            logger.LogError($"Updating failed. Direction with Id = {dto?.Id} doesn't exist in the system.");
            throw new DbUpdateConcurrencyException($"Updating failed. Direction with Id = {dto?.Id} doesn't exist in the system.");
        }

        mapper.Map(dto, direction);
        direction = await repository.Update(direction).ConfigureAwait(false);

        logger.LogInformation($"Direction with Id = {direction?.Id} updated succesfully.");

        return mapper.Map<DirectionDto>(direction);
    }

    private void DirectionValidation(DirectionDto dto)
    {
        if (repository.Get(whereExpression: x => x.Title == dto.Title).Any())
        {
            throw new ArgumentException(localizer["There is already a Direction with such a data."]);
        }
    }

    private async Task<(Expression<Func<Direction, bool>>, Expression<Func<Workshop, bool>>)> BuildPredicate(DirectionFilter filter, bool isAdmins)
    {
        Expression<Func<Direction, bool>> predicate = PredicateBuilder.True<Direction>();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            predicate = predicate
                .And(direction => direction.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        Expression<Func<Workshop, bool>> workshopCountFilter = PredicateBuilder.True<Workshop>();

        if (isAdmins)
        {
            if (currentUserService.IsMinistryAdmin())
            {
                var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
                predicate = predicate
                    .And<Direction>(d => d.InstitutionHierarchies.Any(h => h.InstitutionId == ministryAdmin.InstitutionId));
                workshopCountFilter = workshopCountFilter
                    .And<Workshop>(w => w.InstitutionHierarchy.InstitutionId == ministryAdmin.InstitutionId);
            }
            else if (currentUserService.IsRegionAdmin())
            {
                var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
                predicate = predicate
                    .And<Direction>(d => d.InstitutionHierarchies.Any(h => h.InstitutionId == regionAdmin.InstitutionId));
                workshopCountFilter = workshopCountFilter
                    .And<Workshop>(w => w.InstitutionHierarchy.InstitutionId == regionAdmin.InstitutionId);
            }
        }

        if (!isAdmins)
        {
            workshopCountFilter = workshopCountFilter
                .And<Workshop>(w => w.Address.CATOTTGId == filter.CatottgId);
        }

        return (predicate, workshopCountFilter);
    }
}
