using System.Linq.Expressions;
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
/// <param name="repositoryParent">Repository for parent entity.</param>
/// <param name="currentUserService">Service for managing current user rights.</param>
/// <param name="parentBlockedByAdminLogService">Service for logging parent blocking by an administrator.</param>
/// <param name="repositoryChild">Repository for child entity.</param>
/// <param name="logger">Logger.</param>
/// <param name="userService">Service for Users.</param>
/// <param name="usersRepository">Repository for Users.</param>
public class ParentService(
    IParentRepository repositoryParent,
    ICurrentUserService currentUserService,
    IParentBlockedByAdminLogService parentBlockedByAdminLogService,
    ILogger<ParentService> logger,
    IEntityRepositorySoftDeleted<Guid, Child> repositoryChild,
    IUserService userService,
    IEntityRepositorySoftDeleted<string, User> usersRepository
) : IParentService
{
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

        var newParent = parentCreateDto.ToModel();

        user.IsRegistered = true;
        user.PhoneNumber = parentCreateDto.PhoneNumber;

        newParent.User = user;

        Func<Task<Parent>> operation = async () =>
            await repositoryParent.Create(newParent).ConfigureAwait(false);

        var parent = await repositoryParent.RunInTransaction(operation).ConfigureAwait(false);

        logger.LogInformation("Successfully created Parent with Id = {Id} for UserId = {UserId}", parent.Id, userId);

        return parent.ToDto();
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
        logger.LogDebug("Getting Parent by UserId started. Looking UserId is {Id}", id);

        Expression<Func<Parent, bool>> filter = p => p.UserId == id;

        var parent = (await repositoryParent.GetByFilter(filter)).FirstOrDefault();

        await currentUserService.UserHasRights(new ParentRights(parent?.Id ?? Guid.Empty));

        logger.LogDebug("Successfully got a Parent with UserId = {Id}", id);

        return parent.ToDto();
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
                        .SingleOrDefault();

        return info.ToShortUser();
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

            dto.SetToModel(parent.User);

            logger.LogInformation("Parent with UserId = {ParentId} updated successfully", parent.Id);

            await repositoryParent.SaveChangesAsync();

            return parent.ToShortUser();
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

        async Task operation()
        {
            try
            {
                await repositoryParent.SaveChangesAsync();
                await parentBlockedByAdminLogService.SaveChangesLogAsync(
                    parent.Id,
                    currentUserService.UserId,
                    parentBlockUnblock.Reason,
                    parentBlockUnblock.IsBlocked).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update block status or save log for parent {ParentId}", parent.Id);
                throw new InvalidOperationException($"Failed to update block status or save log for parent {parent.Id}");
            }
        }

        await repositoryParent.RunInTransaction(operation).ConfigureAwait(false);

        logger.LogInformation("Successfully changed Block status of Parent with ParentId = {Id}", parentBlockUnblock.ParentId);

        return Result<bool>.Success(true);
    }
}