using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Parent;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Service with business logic for ParentController.
/// </summary>
public class ParentService : IParentService
{
    private readonly IParentRepository repositoryParent;
    private readonly ICurrentUserService currentUserService;
    private readonly IParentBlockedByAdminLogService parentBlockedByAdminLogService;
    private readonly ILogger<ParentService> logger;
    private readonly IEntityRepositorySoftDeleted<Guid, Child> repositoryChild;
    private readonly IMapper mapper;
    private readonly IUserService userService;
    private readonly IEntityRepositorySoftDeleted<string, User> usersRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentService"/> class.
    /// </summary>
    /// <param name="repositoryParent">Repository for parent entity.</param>
    /// <param name="currentUserService">Service for managing current user rights.</param>
    /// <param name="parentBlockedByAdminLogService">Service for logging parent blocking by an administrator.</param>
    /// <param name="repositoryChild">Repository for child entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="userService">Service for Users.</param>
    /// <param name="usersRepository">Repository for Users.</param>
    public ParentService(
        IParentRepository repositoryParent,
        ICurrentUserService currentUserService,
        IParentBlockedByAdminLogService parentBlockedByAdminLogService,
        ILogger<ParentService> logger,
        IEntityRepositorySoftDeleted<Guid, Child> repositoryChild,
        IMapper mapper,
        IUserService userService,
        IEntityRepositorySoftDeleted<string, User> usersRepository)
    {
        this.repositoryParent = repositoryParent ?? throw new ArgumentNullException(nameof(repositoryParent));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.repositoryChild = repositoryChild ?? throw new ArgumentNullException(nameof(repositoryChild));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.parentBlockedByAdminLogService = parentBlockedByAdminLogService ?? throw new ArgumentNullException(nameof(parentBlockedByAdminLogService));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    /// <inheritdoc/>
    public async Task<ParentDTO> Create(ParentCreateDto parentCreateDto)
    {
        ArgumentNullException.ThrowIfNull(parentCreateDto);

        var userId = currentUserService.UserId;

        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("Unable to create new parent. UserId is null or empty.");
            throw new InvalidOperationException($"Unable to create new parent. UserId is null or empty.");
        }
        // TODO: Should we replace GetById() with GetByFilter() to include the Individual entity in User?
        var user = await usersRepository.GetById(userId).ConfigureAwait(false);

        if (user is null)
        {
            logger.LogError("Unable to create new parent. User with UserId = {UserId} not found.", userId);
            throw new InvalidOperationException($"Unable to create new parent. User with UserId = {userId} not found.");
        }

        if (await repositoryParent.Any(p => p.UserId == userId).ConfigureAwait(false))
        {
            logger.LogError("Unable to create new parent. Parent with UserId = {UserId} already exists.", userId);
            throw new InvalidOperationException($"Unable to create new parent. Parent with UserId = {userId} already exists.");
        }

        logger.LogInformation("Creating Parent for UserId = {UserId} started", userId);

        var newParent = mapper.Map<Parent>(parentCreateDto);

        user.IsRegistered = true;
        user.PhoneNumber = parentCreateDto.PhoneNumber;

        newParent.User = user;
        // TODO: Should we do this operation Create in one transaction?
        var parent = await repositoryParent.Create(newParent).ConfigureAwait(false);

        await repositoryParent.SaveChangesAsync();

        logger.LogInformation("Successfully created Parent with Id = {Id} for UserId = {UserId}", parent.Id, userId);

        return mapper.Map<ParentDTO>(parent);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation("Deleting Parent with Id = {Id} started", id);

        await currentUserService.UserHasRights(new ParentRights(id));

        var entity = await repositoryParent.GetById(id).ConfigureAwait(false);

        if (entity is null)
        {
            var message = $"Parent with Id = {id} doesn't exist in the system.";
            logger.LogError(message);
            throw new ArgumentException(message, nameof(id));
        }

        await repositoryParent.RunInTransaction(async () =>
        {
            await repositoryParent.Delete(entity).ConfigureAwait(false);
            await userService.Delete(entity.UserId).ConfigureAwait(false);
        });

        logger.LogInformation("Parent with Id = {Id} successfully deleted", id);
    }

    /// <inheritdoc/>
    public async Task<ParentDTO> GetByUserId(string id)
    {
        logger.LogInformation("Getting Parent by UserId started. Looking UserId is {Id}", id);

        Expression<Func<Parent, bool>> filter = p => p.UserId == id;

        var parent = (await repositoryParent.GetByFilter(filter)).FirstOrDefault();

        await currentUserService.UserHasRights(new ParentRights(parent?.Id ?? Guid.Empty));

        logger.LogInformation("Successfully got a Parent with UserId = {Id}", id);

        return mapper.Map<ParentDTO>(parent);
    }

    /// <inheritdoc/>
    public async Task<ShortUserDto> GetPersonalInfoByUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException(@"User Id must be non empty value", nameof(userId));
        }

        var includeFunc = (IQueryable<Parent> p) => p.Include(p => p.User);

        var info = (await repositoryParent.GetByFilter(
                        whereExpression: x => x.UserId == userId,
                        includeExpression: includeFunc))
                        .FirstOrDefault();

        return mapper.Map<ShortUserDto>(info);
    }

    /// <inheritdoc/>
    public async Task<ShortUserDto> Update(BaseUpdateUserDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        logger.LogDebug("Updating Parent with User Id = {UserId} started", dto.Id);

        try
        {
            var includeFunc = (IQueryable<Parent> p) => p.Include(p => p.User);

            var parent = (await repositoryParent.GetByFilter(
                            whereExpression: x => x.UserId == dto.Id,
                            includeExpression: includeFunc))
                            .FirstOrDefault();

            if (parent is null)
            {
                throw new ArgumentException("No parent with id, given in model was not found");
            }

            await currentUserService.UserHasRights(new ParentRights(parent.Id));

            mapper.Map(dto, parent.User);

            logger.LogInformation("Parent with UserId = {ParentId} updated successfully", parent.Id);

            await repositoryParent.SaveChangesAsync();

            return mapper.Map<ShortUserDto>(parent);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Updating Parent with UserId = {ParentId} failed", dto.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> BlockUnblockParent(BlockUnblockParentDto parentBlockUnblock)
    {
        ArgumentNullException.ThrowIfNull(parentBlockUnblock);
        logger.LogInformation("Changing Block status of Parent by ParentId started. Looking ParentId is {Id}", parentBlockUnblock.ParentId);

        var includeFunc = (IQueryable<Parent> p) => p.Include(p => p.User);

        var parent = await repositoryParent.GetByIdWithDetails(
            id: parentBlockUnblock.ParentId,
            includeExpression: includeFunc)
            .ConfigureAwait(false);

        if (parent is null || parent.User.IsBlocked == parentBlockUnblock.IsBlocked)
        {
            logger.LogInformation($"Changing Block status of Parent aborted. " +
                $"{(parent == null ? "Parent not found." : "Parent already blocked/unblocked.")}");
            return Result<bool>.Success(true);
        }

        parent.User.IsBlocked = parentBlockUnblock.IsBlocked;
        // TODO: Should we do these two operations in one transaction?
        await repositoryParent.SaveChangesAsync();
        await parentBlockedByAdminLogService.SaveChangesLogAsync(
            parent.Id,
            currentUserService.UserId,
            parentBlockUnblock.Reason,
            parentBlockUnblock.IsBlocked).ConfigureAwait(false);
        logger.LogInformation("Successfully changed Block status of Parent with ParentId = {Id}", parentBlockUnblock.ParentId);
        return Result<bool>.Success(true);
    }
}