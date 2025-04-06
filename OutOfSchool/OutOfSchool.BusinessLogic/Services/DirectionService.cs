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
    public async Task<Result<DirectionDto>> Create(DirectionDto dto)
    {
        logger.LogDebug("Direction creating was started.");

        var direction = mapper.Map<Direction>(dto);

        var validationErrors = await DirectionValidation(dto).ConfigureAwait(false);
        if (validationErrors.Any())
        {
            return Result<DirectionDto>.Failed(validationErrors.ToArray());
        }

        var newDirection = await repository.Create(direction).ConfigureAwait(false);

        logger.LogDebug("Direction with Id = {id} created successfully.", newDirection?.Id);

        return Result<DirectionDto>.Success(mapper.Map<DirectionDto>(newDirection));
    }

    /// <inheritdoc/>
    public async Task<Result<DirectionDto>> Delete(long id)
    {
        logger.LogDebug("Deleting Direction with Id = {id} started.", id);

        var direction = await repository.GetById(id).ConfigureAwait(false);

        if (direction == null)
        {
            return Result<DirectionDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"Direction with Id = {id} does not exist.",
            });
        }

        var workShops = await repositoryWorkshop
            .GetByFilter(w => w.InstitutionHierarchy.SubDirections.Any(d => d.DirectionId == id))
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

            logger.LogDebug("Direction with Id = {id} succesfully deleted.", id);

            return Result<DirectionDto>.Success(mapper.Map<DirectionDto>(direction));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Deleting failed. Direction with Id = {id} doesn't exist in the system.", id);
            return Result<DirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Deleting Direction with Id = {id} failed"
            });
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DirectionDto>> GetAll()
    {
        logger.LogDebug("Getting all Directions started.");

        var directions = await repository.GetAll().ConfigureAwait(false);

        logger.LogDebug("{Count} records were successfully received from the Direction table.", directions.Count());

        return directions.OrderBy(x => x.Title).Select(entity => mapper.Map<DirectionDto>(entity)).ToList();
    }

    /// <inheritdoc/>
    public async Task<SearchResult<DirectionDto>> GetByFilter(DirectionFilter filter, bool isAdmins)
    {
        logger.LogDebug("Getting Directions by filter started.");

        var (predicate, workshopCountFilter) = await BuildPredicate(filter, isAdmins).ConfigureAwait(false);

        var count = await repository.Count(predicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Direction, object>>, SortDirection>
        {
            { x => x.Title, SortDirection.Ascending },
        };

        var directions = await repository
            .Get(skip: filter.From, take: filter.Size, whereExpression: predicate, orderBy: sortExpression)
            .Include(d => d.SubDirections)
            .ToListAsync();

        var workshopCount = await repositoryWorkshop
            .Get(whereExpression: workshopCountFilter
                .And(w => w.InstitutionHierarchy.SubDirections.Any(d => directions.Contains(d.Direction))))
            .SelectMany(w => w.InstitutionHierarchy.SubDirections.Select(d => d.Direction))
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

        logger.LogDebug("{Count} records were successfully received from the Direction table.", directionsWorkshops.Count);

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
        logger.LogDebug("Getting Direction by Id started. Looking Id = {id}.", id);

        var direction = await repository.GetById((int)id).ConfigureAwait(false);

        if (direction == null)
        {
            logger.LogError("Direction with Id = {id} doesn't exist in the system.", id);
            return null;
        }

        logger.LogDebug("Successfully got a Direction with Id = {id}.", id);

        return mapper.Map<DirectionDto>(direction);
    }

    /// <inheritdoc/>
    public async Task<Result<DirectionDto>> Update(DirectionDto dto)
    {
        logger.LogDebug("Updating Direction with Id = {id} started.", dto?.Id);

        if (dto == null)
        {
            logger.LogError("Updating failed. Dto is null");
            return Result<DirectionDto>.Failed(new OperationError
            {
                Code = "400",
                Description = $"Dto is null.",
            });
        }

        var direction = await repository.GetById(dto.Id).ConfigureAwait(false);

        if (direction is null)
        {
            logger.LogError("Updating failed. Direction with Id = {id} doesn't exist in the system.", dto.Id);
            return Result<DirectionDto>.Failed(new OperationError
            {
                Code = "404",
                Description = $"Direction with Id = {dto.Id} does not exist.",
            });
        }

        mapper.Map(dto, direction);
        direction = await repository.Update(direction).ConfigureAwait(false);

        logger.LogDebug("Direction with Id = {id} updated succesfully.", direction?.Id);

        return Result<DirectionDto>.Success(mapper.Map<DirectionDto>(direction));
    }

    private async Task<List<OperationError>> DirectionValidation(DirectionDto dto)
    {
        var errors = new List<OperationError>();

        if (await repository.Get(whereExpression: x => x.Title == dto.Title).AnyAsync())
        {
            logger.LogWarning(localizer["There is already a Direction with such a data."]);

            errors.Add(new OperationError()
            {
                Code = "400",
                Description = localizer["There is already a Direction with such a data."]
            });
        }

        return errors;
    }

    private async Task<(Expression<Func<Direction, bool>>, Expression<Func<Workshop, bool>>)> BuildPredicate(DirectionFilter filter, bool isAdmins)
    {
        Expression<Func<Direction, bool>> predicate = PredicateBuilder.True<Direction>();

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            predicate = predicate
                .And(direction => direction.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase) ||
                direction.SubDirections.Any(s => s.Title.Contains(filter.Name, StringComparison.InvariantCultureIgnoreCase)));
        }

        Expression<Func<Workshop, bool>> workshopCountFilter = PredicateBuilder.True<Workshop>();

        if (isAdmins)
        {
            if (currentUserService.IsMinistryAdmin())
            {
                var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
                predicate = predicate
                    .And<Direction>(d => d.SubDirections.
                    Any(s => s.InstitutionHierarchies.Any(h => h.InstitutionId == ministryAdmin.InstitutionId)));
                workshopCountFilter = workshopCountFilter
                    .And<Workshop>(w => w.InstitutionHierarchy.InstitutionId == ministryAdmin.InstitutionId);
            }
            else if (currentUserService.IsRegionAdmin())
            {
                var regionAdmin = await regionAdminService.GetByUserId(currentUserService.UserId);
                predicate = predicate
                    .And<Direction>(d => d.SubDirections
                    .Any(s => s.InstitutionHierarchies.Any(h => h.InstitutionId == regionAdmin.InstitutionId)));
                workshopCountFilter = workshopCountFilter
                    .And<Workshop>(w => w.InstitutionHierarchy.InstitutionId == regionAdmin.InstitutionId);
            }
        }
        else {
            workshopCountFilter = workshopCountFilter
                .And<Workshop>(w => w.Contacts.Any(c => c.IsDefault && c.Address.CATOTTGId == filter.CatottgId));
        }

        return (predicate, workshopCountFilter);
    }
}
