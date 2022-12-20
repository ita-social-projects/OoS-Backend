using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Service with business logic for ParentController.
/// </summary>
public class ParentService : IParentService
{
    private readonly IParentRepository repositoryParent;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<ParentService> logger;
    private readonly IEntityRepository<Guid, Child> repositoryChild;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentService"/> class.
    /// </summary>
    /// <param name="repositoryParent">Repository for parent entity.</param>
    /// <param name="currentUserService">Service for managing current user rights.</param>
    /// <param name="repositoryChild">Repository for child entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public ParentService(
        IParentRepository repositoryParent,
        ICurrentUserService currentUserService,
        ILogger<ParentService> logger,
        IEntityRepository<Guid, Child> repositoryChild,
        IMapper mapper)
    {
        this.repositoryParent = repositoryParent ?? throw new ArgumentNullException(nameof(repositoryParent));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.repositoryChild = repositoryChild ?? throw new ArgumentNullException(nameof(repositoryChild));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
    public async Task<SearchResult<ParentDTO>> GetByFilter(SearchStringFilter filter)
    {
        logger.LogInformation("Getting all Parents started (by filter)");

        if (!currentUserService.IsAdmin())
        {
            throw new UnauthorizedAccessException("User has no rights to perform operation");
        }

        filter ??= new SearchStringFilter();

        var filterPredicate = PredicateBuild(filter);

        var count = await repositoryParent.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Parent, object>>, SortDirection>
        {
            {x => x.User.FirstName, SortDirection.Ascending},
        };

        var parents = await repositoryParent
            .Get(filter.From, filter.Size, string.Empty, filterPredicate, sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation("All {Count} records were successfully received from the Parent table", parents.Count);

        var result = new SearchResult<ParentDTO>()
        {
            TotalAmount = count,
            Entities = parents.Select(entity => mapper.Map<ParentDTO>(entity)).ToList(),
        };

        return result;
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
    public async Task<ParentPersonalInfo> GetPersonalInfoByUserId(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException(@"User Id must be non empty value", nameof(userId));
        }

        var info = (await repositoryParent.GetByFilter(
            x => x.UserId == userId,
            $"{nameof(Parent.User)}")).FirstOrDefault();

        await currentUserService.UserHasRights(new ParentRights(info?.Id ?? Guid.Empty));

        return mapper.Map<ParentPersonalInfo>(info);
    }

    /// <inheritdoc/>
    public async Task<ParentDTO> GetById(Guid id)
    {
        logger.LogInformation("Getting Parent by Id started. Looking Id = {Id}", id);

        if (!currentUserService.IsAdmin())
        {
            await currentUserService.UserHasRights(new ParentRights(id));
        }

        var parent = await repositoryParent.GetById(id);

        logger.LogInformation("Successfully got a Parent with Id = {Id}", id);

        return mapper.Map<ParentDTO>(parent);
    }

    /// <inheritdoc/>
    public async Task<ParentPersonalInfo> Update(ParentPersonalInfo dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
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

            mapper.Map((ShortUserDto)dto, parent.User);
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

            return mapper.Map<ParentPersonalInfo>(parent);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Updating Parent with UserId = {ParentId} failed", dto.Id);
            throw;
        }
    }

    private Expression<Func<Parent, bool>> PredicateBuild(SearchStringFilter filter)
    {
        var predicate = PredicateBuilder.True<Parent>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<Parent>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.User.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.PhoneNumber.Contains(word, StringComparison.InvariantCulture));
            }

            predicate = predicate.And(tempPredicate);
        }

        return predicate;
    }
}