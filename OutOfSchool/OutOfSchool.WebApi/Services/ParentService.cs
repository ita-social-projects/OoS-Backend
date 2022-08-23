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
    private readonly IEntityRepository<string, User> repositoryUser;
    private readonly ILogger<ParentService> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IEntityRepository<Guid, Child> repositoryChild;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentService"/> class.
    /// </summary>
    /// <param name="repositoryParent">Repository for parent entity.</param>
    /// <param name="repositoryUser">Repository for user entity.</param>
    /// <param name="repositoryChild">Repository for child entity.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="mapper">Mapper.</param>
    public ParentService(
        IParentRepository repositoryParent,
        IEntityRepository<string, User> repositoryUser,
        ILogger<ParentService> logger,
        IStringLocalizer<SharedResource> localizer,
        IEntityRepository<Guid, Child> repositoryChild,
        IMapper mapper)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.repositoryParent = repositoryParent ?? throw new ArgumentNullException(nameof(repositoryParent));
        this.repositoryUser = repositoryUser ?? throw new ArgumentNullException(nameof(repositoryUser));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.repositoryChild = repositoryChild ?? throw new ArgumentNullException(nameof(repositoryChild));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<ParentDTO> Create(ParentDTO dto)
    {
        logger.LogInformation("Parent creating was started");

        Func<Task<Parent>> operation = async () => await repositoryParent.Create(mapper.Map<Parent>(dto)).ConfigureAwait(false);

        var newParent = await repositoryParent.RunInTransaction(operation).ConfigureAwait(false);

        logger.LogInformation($"Parent with Id = {newParent?.Id} created successfully.");

        return mapper.Map<ParentDTO>(newParent);
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation($"Deleting Parent with Id = {id} started.");

        var entity = new Parent() { Id = id };

        try
        {
            await repositoryParent.Delete(entity).ConfigureAwait(false);

            logger.LogInformation($"Parent with Id = {id} succesfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting failed. Parent with Id = {id} doesn't exist in the system.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ParentDTO>> GetAll()
    {
        logger.LogInformation("Getting all Parents started.");

        var parents = await this.repositoryParent.GetAll().ConfigureAwait(false);

        logger.LogInformation(!parents.Any()
            ? "Parent table is empty."
            : $"All {parents.Count()} records were successfully received from the Parent table");

        return parents.Select(parent => mapper.Map<ParentDTO>(parent)).ToList();
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ParentDTO>> GetByFilter(SearchStringFilter filter)
    {
        logger.LogInformation("Getting all Parents started (by filter).");

        filter ??= new SearchStringFilter();

        var filterPredicate = PredicateBuild(filter);

        int count = await repositoryParent.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Parent, object>>, SortDirection>
    {
        { x => x.User.FirstName, SortDirection.Ascending },
    };

        var parents = await repositoryParent
            .Get(filter.From, filter.Size, string.Empty, filterPredicate, sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation(!parents.Any()
            ? "Parents table is empty."
            : $"All {parents.Count} records were successfully received from the Parent table");

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
        logger.LogInformation($"Getting Parent by UserId started. Looking UserId is {id}.");

        Expression<Func<Parent, bool>> filter = p => p.UserId == id;

        var parents = await repositoryParent.GetByFilter(filter).ConfigureAwait(false);

        if (!parents.Any())
        {
            throw new ArgumentException(localizer["There is no Parent in the Db with such User id"], nameof(id));
        }

        logger.LogInformation($"Successfully got a Parent with UserId = {id}.");

        return mapper.Map<ParentDTO>(parents.First());
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

        if (info is null)
        {
            throw new ArgumentException(@"No user with such id was found", nameof(userId));
        }

        return mapper.Map<ParentPersonalInfo>(info);
    }

    /// <inheritdoc/>
    public async Task<ParentDTO> GetById(Guid id)
    {
        logger.LogInformation($"Getting Parent by Id started. Looking Id = {id}.");

        var parent = await repositoryParent.GetById(id).ConfigureAwait(false);

        if (parent == null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                localizer["The id cannot be greater than number of table entities."]);
        }

        logger.LogInformation($"Successfully got a Parent with Id = {id}.");

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

            mapper.Map((ShortUserDto)dto, parent.User);
            parent.Gender = dto.Gender;
            parent.DateOfBirth = dto.DateOfBirth;

            logger.LogInformation("Parent with UserId = {ParentId} updated successfully", parent.Id);

            var child = (await repositoryChild.GetByFilter(c => c.Parent.UserId == dto.Id && c.IsParent)).SingleOrDefault();

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
            logger.LogError(ex, "Updating Parent with UserId = {ParentId} failed", dto?.Id);
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