using System.Linq.Expressions;
using AutoMapper;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Parent;

namespace OutOfSchool.WebApi.Services;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentService"/> class.
    /// </summary>
    /// <param name="repositoryParent">Repository for parent entity.</param>
    /// <param name="currentUserService">Service for managing current user rights.</param>
    /// <param name="parentBlockedByAdminLogService">Service for logging parent blocking by an administrator.</param>
    /// <param name="repositoryChild">Repository for child entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public ParentService(
        IParentRepository repositoryParent,
        ICurrentUserService currentUserService,
        IParentBlockedByAdminLogService parentBlockedByAdminLogService,
        ILogger<ParentService> logger,
        IEntityRepositorySoftDeleted<Guid, Child> repositoryChild,
        IMapper mapper)
    {
        this.repositoryParent = repositoryParent ?? throw new ArgumentNullException(nameof(repositoryParent));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.repositoryChild = repositoryChild ?? throw new ArgumentNullException(nameof(repositoryChild));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.parentBlockedByAdminLogService = parentBlockedByAdminLogService
            ?? throw new ArgumentNullException(nameof(parentBlockedByAdminLogService));
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation("Deleting Parent with Id = {Id} started", id);

        await currentUserService.UserHasRights(new ParentRights(id));

        var entity = new Parent() {Id = id};

        try
        {
            await repositoryParent.Delete(entity).ConfigureAwait(false);

            logger.LogInformation("Parent with Id = {Id} successfully deleted", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Deleting failed. Parent with Id = {Id} doesn't exist in the system", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ParentDTO> GetByUserId(string id)
    {
        logger.LogInformation("Getting Parent by UserId started. Looking UserId is {Id}", id);

        Expression<Func<Parent, bool>> filter = p => p.UserId == id;

        var parents = await repositoryParent.GetByFilter(filter);

        var parent = parents.FirstOrDefault();

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

        var info = (await repositoryParent.GetByFilter(
            x => x.UserId == userId,
            $"{nameof(Parent.User)}")).FirstOrDefault();

        await currentUserService.UserHasRights(new ParentRights(info?.Id ?? Guid.Empty));

        return mapper.Map<ShortUserDto>(info);
    }

    /// <inheritdoc/>
    public Task<ShortUserDto> Update(ShortUserDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Gender is null || dto.DateOfBirth is null)
        {
            throw new ArgumentException($"{nameof(dto.Gender)} and/or {nameof(dto.DateOfBirth)} are required but were not provided.");
        }

        return ExecuteUpdate(dto);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> BlockParent(ParentBlockByAdminDto parentBlockByAdmin)
    {
        logger.LogInformation("Changing Block status of Parent by ParentId started. Looking ParentId is {Id}", parentBlockByAdmin.ParentId);
        var parent = await repositoryParent.GetByIdWithDetails(parentBlockByAdmin.ParentId, "User").ConfigureAwait(false);
        if (parent is null)
        {
            return Result<bool>.Failed(new OperationError
            {
                Code = "404",
                Description = $"ParentId not found: {parentBlockByAdmin.ParentId}.",
            });
        }

        if (parent.User.IsBlocked == parentBlockByAdmin.ToggleBlock)
        {
            return Result<bool>.Failed(new OperationError
            {
                Code = "400",
                Description = $"ParentId is already {(parentBlockByAdmin.ToggleBlock ? "blocked" : "unblocked")}: {parent.Id}.",
            });
        }

        parent.User.IsBlocked = parentBlockByAdmin.ToggleBlock;
        await repositoryParent.UnitOfWork.CompleteAsync();
        await parentBlockedByAdminLogService.SaveChangesLogAsync(
            parent.Id,
            currentUserService.UserId,
            parentBlockByAdmin.Reason,
            parentBlockByAdmin.ToggleBlock).ConfigureAwait(false);
        logger.LogInformation("Successfully changed Block status of Parent with ParentId = {Id}", parentBlockByAdmin.ParentId);
        return Result<bool>.Success(true);
    }

    private async Task<ShortUserDto> ExecuteUpdate(ShortUserDto dto)
    {
        logger.LogDebug("Updating Parent with User Id = {UserId} started", dto.Id);

        try
        {
            var parent = (await repositoryParent.GetByFilter(x => x.UserId == dto.Id))
                .FirstOrDefault();

            if (parent is null)
            {
                throw new ArgumentException("No parent with id, given in model was not found");
            }

            await currentUserService.UserHasRights(new ParentRights(parent.Id));

            mapper.Map(dto, parent.User);
            parent.Gender = dto.Gender;
            parent.DateOfBirth = dto.DateOfBirth;

            logger.LogInformation("Parent with UserId = {ParentId} updated successfully", parent.Id);

            var child = (await repositoryChild.GetByFilter(c => c.Parent.UserId == dto.Id && c.IsParent))
                .SingleOrDefault();

            if (child is not null)
            {
                child.FirstName = dto.FirstName;
                child.MiddleName = dto.MiddleName;
                child.LastName = dto.LastName;
                child.Gender = dto.Gender;
                child.DateOfBirth = dto.DateOfBirth;
            }

            await repositoryParent.UnitOfWork.CompleteAsync();

            return mapper.Map<ShortUserDto>(parent);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Updating Parent with UserId = {ParentId} failed", dto.Id);
            throw;
        }
    }
}