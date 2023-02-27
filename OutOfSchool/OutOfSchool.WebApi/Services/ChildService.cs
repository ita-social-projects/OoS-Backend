using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Google.Type;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.SocialGroup;
using OutOfSchool.WebApi.Util;
using DateTime = System.DateTime;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Child entity.
/// </summary>
public class ChildService : IChildService
{
    private readonly IEntityRepository<Guid, Child> childRepository;
    private readonly IParentRepository parentRepository;
    private readonly IApplicationRepository applicationRepository;
    private readonly IEntityRepository<long, SocialGroup> socialGroupRepository;
    private readonly ILogger<ChildService> logger;
    private readonly IMapper mapper;
    private readonly IOptions<ParentConfig> parentConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildService"/> class.
    /// </summary>
    /// <param name="childRepository">Repository for the Child entity.</param>
    /// <param name="parentRepository">Repository for the Parent entity.</param>
    /// <param name="socialGroupRepository">Repository for the social groups.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Automapper DI service.</param>
    /// <param name="parentConfig">Parent configuration.</param>
    public ChildService(
        IEntityRepository<Guid, Child> childRepository,
        IParentRepository parentRepository,
        IEntityRepository<long, SocialGroup> socialGroupRepository,
        ILogger<ChildService> logger,
        IMapper mapper,
        IApplicationRepository applicationRepository,
        IOptions<ParentConfig> parentConfig)
    {
        this.childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        this.parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
        this.socialGroupRepository =
            socialGroupRepository ?? throw new ArgumentNullException(nameof(socialGroupRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.applicationRepository =
            applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
        this.parentConfig = parentConfig ?? throw new ArgumentNullException(nameof(parentConfig));
    }

    /// <inheritdoc/>
    public async Task<ChildDto> CreateChildForUser(ChildDto childDto, string userId)
    {
        ValidateChildDto(childDto);
        ValidateUserId(userId);

        logger.LogDebug(
            $"Started creation of a new child with {nameof(Child.ParentId)}:{childDto.ParentId}, {nameof(userId)}:{userId}.");

        var parent =
            (await parentRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false)).SingleOrDefault()
            ?? throw new UnauthorizedAccessException(
                $"Trying to create a new child the Parent with {nameof(userId)}:{userId} was not found.");

        if (childDto.ParentId != parent.Id)
        {
            logger.LogWarning(
                $"Prevented action! User:{userId} with {nameof(Child.ParentId)}:{parent.Id} was trying to create a new child with not his own {nameof(Child.ParentId)}:{childDto.ParentId}.");
            childDto.ParentId = parent.Id;
        }

        if (childDto.IsParent)
        {
            throw new ArgumentException($"Forbidden to create child which related to the parent.");
        }

        async Task<Child> CreateChild()
        {
            var child = mapper.Map<Child>(childDto);
            child.Id = default;
            child.SocialGroups = new List<SocialGroup>();

            var newChild = await childRepository.Create(child).ConfigureAwait(false);

            await UpdateSocialGroups(newChild, childDto.SocialGroups).ConfigureAwait(false);

            await CompleteChildChangesAsync().ConfigureAwait(false);

            return newChild;
        }

        var newChild = await childRepository
           .RunInTransaction(CreateChild).ConfigureAwait(false);

        logger.LogDebug(
            $"Child with Id:{newChild.Id} ({nameof(Child.ParentId)}:{newChild.ParentId}, {nameof(userId)}:{userId}) was created successfully.");

        return mapper.Map<ChildDto>(newChild);
    }

    /// <inheritdoc/>
    public async Task<ChildrenCreationResultDto> CreateChildrenForUser(List<ChildDto> childrenDtos, string userId)
    {
        var parent = (await parentRepository
            .GetByFilter(p => p.UserId == userId)
            .ConfigureAwait(false))
            .SingleOrDefault()
            ?? throw new UnauthorizedAccessException($"Trying to create a new children the Parent with {nameof(userId)}:{userId} was not found.");

        var parentChildrenCount = (await GetChildrenListByParentId(parent.Id, false).ConfigureAwait(false))?.Count;

        var children = new ChildrenCreationResultDto()
        {
            Parent = mapper.Map<ParentDTO>(parent),
        };

        foreach (var childDto in childrenDtos)
        {
            try
            {
                if (parentChildrenCount < parentConfig.Value.ChildrenMaxNumber)
                {
                    var child = await CreateChildForUser(childDto, userId).ConfigureAwait(false);
                    children.ChildrenCreationResults.Add(CreateChildResult(child));
                    parentChildrenCount++;
                }
                else
                {
                    children.ChildrenCreationResults
                        .Add(CreateChildResult(
                            childDto,
                            false,
                            $"Refused to create a new child with {nameof(Child.ParentId)}:{childDto.ParentId}, {nameof(userId)}:{userId}: " +
                            $"the limit ({parentConfig.Value.ChildrenMaxNumber}) of the children for parents was reached."));
                }
            }
            catch (Exception ex) when (ex is ArgumentNullException
                || ex is ArgumentException
                || ex is UnauthorizedAccessException
                || ex is DbUpdateException)
            {
                children.ChildrenCreationResults.Add(CreateChildResult(childDto, false, ex.Message));
                logger.LogDebug(
                    $"There is an error while creating a new child with {nameof(Child.ParentId)}:{childDto.ParentId}, {nameof(userId)}:{userId}: {ex.Message}.");
            }
            catch (Exception ex)
            {
                children.ChildrenCreationResults.Add(CreateChildResult(childDto, false));
                logger.LogDebug(
                    $"There is an error while creating a new child with {nameof(Child.ParentId)}:{childDto.ParentId}, {nameof(userId)}:{userId}: {ex.Message}.");
            }
        }

        ChildCreationResult CreateChildResult(ChildDto childDto, bool isSuccess = true, string message = null)
        {
            return new ChildCreationResult()
            {
                Child = childDto,
                IsSuccess = isSuccess,
                Message = message,
            };
        }

        return children;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ChildDto>> GetByFilter(ChildSearchFilter filter)
    {
        filter ??= new ChildSearchFilter();

        logger.LogDebug(
            $"Getting all Children started. Amount of children to take: {filter.Size}, skip first: {filter.From}.");

        this.ValidateOffsetFilter(filter);

        var filterPredicate = PredicateBuild(filter);

        var totalAmount = await childRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
        {
            { x => x.Id, SortDirection.Ascending },
        };

        var children = await childRepository.Get(filter.From, filter.Size, $"{nameof(Child.SocialGroups)}",
                filterPredicate, sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug(children.Any()
            ? $"{children.Count} children from {totalAmount} were successfully received. Skipped records: {filter.From}. Order: by Child.Id."
            : $"There is no child in the Children table. Skipped records: {filter.From}. Order: by Child.Id.");

        var searchResult = new SearchResult<ChildDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ChildDto>>(children),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<List<ShortEntityDto>> GetChildrenListByParentId(Guid parentId, bool? isParent)
    {
        logger.LogDebug($"Getting ChildrenList with ParentId: {parentId} started.");

        Expression<Func<Child, bool>> func = child => child.ParentId == parentId;

        if (isParent is not null)
        {
            func = func.And(child => child.IsParent == isParent);
        }

        var children = await childRepository.GetByFilter(func).ConfigureAwait(false);
        var result = mapper.Map<List<ShortEntityDto>>(children).OrderBy(entity => entity.Title).ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<ChildDto> GetByIdAndUserId(Guid id, string userId)
    {
        this.ValidateUserId(userId);

        logger.LogDebug($"User:{userId} is trying to get the child with id: {id}.");

        var child = (await childRepository.GetByFilter(child => child.Id == id, $"{nameof(Child.Parent)}")
                        .ConfigureAwait(false)).SingleOrDefault()
                    ?? throw new UnauthorizedAccessException(
                        $"User:{userId} is trying to get an unexisting child with id: {id}.");

        if (child.Parent.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                $"User{userId} is trying to get not his/her own child with id: {id}.");
        }

        logger.LogDebug($"User:{userId} successfully got the child with id: {id}.");

        return mapper.Map<ChildDto>(child);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ChildDto>> GetByParentIdOrderedByFirstName(Guid parentId, OffsetFilter offsetFilter)
    {
        ValidateOffsetFilter(offsetFilter);

        logger.LogDebug(
            $"Getting Children with ParentId: {parentId} started. Amount of children to take: {offsetFilter.Size}, skip first: {offsetFilter.From}.");

        var totalAmount = await childRepository.Count(x => x.ParentId == parentId).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
        {
            { x => x.FirstName, SortDirection.Ascending },
        };

        var children = await childRepository
            .Get(offsetFilter.From, offsetFilter.Size, "SocialGroups", x => x.ParentId == parentId, sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug(children.Any()
            ? $"{children.Count} children with ParentId: {parentId} were successfully received. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
            : $"There is no child with ParentId: {parentId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

        var searchResult = new SearchResult<ChildDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ChildDto>>(children),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ChildDto>> GetByUserId(string userId, bool isGetParent, OffsetFilter offsetFilter)
    {
        this.ValidateUserId(userId);
        this.ValidateOffsetFilter(offsetFilter);

        logger.LogDebug($"Getting Child's for User started. Looking UserId = {userId}.");

        Expression<Func<Child, bool>> predicate = x => x.Parent.UserId == userId;

        if (!isGetParent)
        {
            predicate = predicate.And(x => !x.IsParent);
        }

        var totalAmount = await childRepository.Count(predicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
        {
            { x => x.FirstName, SortDirection.Ascending },
        };

        var children = await childRepository
            .Get(offsetFilter.From, offsetFilter.Size, string.Empty, predicate, sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug(children.Any()
            ? $"{children.Count} children for User:{userId} were successfully received. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
            : $"There is no child for User:{userId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

        var searchResult = new SearchResult<ChildDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ChildDto>>(children),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<SearchResult<ChildDto>> GetApprovedByWorkshopId(Guid workshopId, OffsetFilter offsetFilter)
    {
        ValidateOffsetFilter(offsetFilter);

        logger.LogDebug(
            $"Getting Children by WorkshopId: {workshopId} started. Amount of children to take: {offsetFilter.Size}, skip first: {offsetFilter.From}.");

        var applications = await applicationRepository
            .GetByFilter(p => p.WorkshopId == workshopId && p.Status == ApplicationStatus.Approved)
            .ConfigureAwait(false);
        var childrenGuids = new HashSet<Guid>(applications.Select(app => app.ChildId));

        var totalAmount = childrenGuids.Count;

        var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
        {
            { x => x.FirstName, SortDirection.Ascending },
        };

        var children = await childRepository
            .Get(offsetFilter.From, offsetFilter.Size, string.Empty, x => childrenGuids.Contains(x.Id), sortExpression)
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogDebug(children.Any()
            ? $"{children.Count} approved children with WorkshopId: {workshopId} were successfully received. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
            : $"There is no approved child with WorkshopId: {workshopId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

        var searchResult = new SearchResult<ChildDto>()
        {
            TotalAmount = totalAmount,
            Entities = mapper.Map<List<ChildDto>>(children),
        };

        return searchResult;
    }

    /// <inheritdoc/>
    public async Task<ChildDto> UpdateChildCheckingItsUserIdProperty(ChildDto childDto, string userId)
    {
        this.ValidateChildDto(childDto);
        this.ValidateUserId(userId);

        logger.LogDebug($"Updating the child with Id: {childDto.Id} and {nameof(userId)}: {userId} started.");

        var child = (await childRepository
                        .GetByFilter(c => c.Id == childDto.Id, $"{nameof(Child.Parent)},{nameof(Child.SocialGroups)}")
                        .ConfigureAwait(false)).SingleOrDefault()
                    ?? throw new InvalidOperationException(
                        $"User: {userId} is trying to update not existing Child (Id = {childDto.Id}).");

        if (child.Parent.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                $"User: {userId} is trying to update not his own child. Child Id = {childDto.Id}");
        }

        if (child.IsParent || childDto.IsParent)
        {
            throw new ArgumentException($"Forbidden to update child which related to the parent.");
        }

        if (childDto.ParentId != child.ParentId)
        {
            logger.LogWarning(
                $"Prevented action! User:{userId} with {nameof(Child.ParentId)}:{child.ParentId} was trying to update his child with not his own {nameof(Child.ParentId)}:{childDto.ParentId}.");
            childDto.ParentId = child.ParentId;
        }

        mapper.Map(childDto, child);

        await UpdateSocialGroups(child, childDto.SocialGroups).ConfigureAwait(false);

        await CompleteChildChangesAsync().ConfigureAwait(false);

        logger.LogDebug("Child with Id = {ChildId} was updated successfully", child.Id);

        return mapper.Map<ChildDto>(child);
    }

    /// <inheritdoc/>
    public async Task DeleteChildCheckingItsUserIdProperty(Guid id, string userId)
    {
        this.ValidateUserId(userId);

        logger.LogDebug($"Deleting the child with Id: {id} and {nameof(userId)}: {userId} started.");

        var child = await childRepository.GetByFilterNoTracking(c => c.Id == id, $"{nameof(Child.Parent)}")
                        .SingleOrDefaultAsync()
                        .ConfigureAwait(false)
                    ?? throw new UnauthorizedAccessException(
                        $"User: {userId} is trying to delete not existing Child (Id = {id}).");

        if (child.Parent.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                $"User: {userId} is not authorized to delete not his own child. Child Id = {id}");
        }

        if (child.IsParent)
        {
            throw new ArgumentException($"Forbidden to delete child which related to the parent.");
        }

        await childRepository.Delete(child).ConfigureAwait(false);

        logger.LogDebug($"Child with Id = {id} succesfully deleted.");
    }

    private static Expression<Func<Child, bool>> PredicateBuild(ChildSearchFilter filter)
    {
        var predicate = PredicateBuilder.True<Child>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<Child>();
            if (filter.SearchString.Length >= 3)
            {
                foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (word.Any(c => char.IsLetter(c)))
                    {
                        tempPredicate = tempPredicate.Or(
                            x => x.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                                 || x.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                                 || x.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                                 || x.Parent.User.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        string phoneNumber = word.Where(c => char.IsNumber(c)).ToString();
                        if (phoneNumber.Length > 0)
                        {
                            tempPredicate = tempPredicate.Or(
                                x => x.Parent.User.PhoneNumber.Contains(word, StringComparison.InvariantCulture));
                        }
                    }
                }
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.IsParent is not null)
        {
            predicate = predicate.And(c => c.IsParent == filter.IsParent);
        }

        return predicate;
    }

    private void ValidateChildDto(ChildDto childDto)
    {
        if (childDto == null)
        {
            throw new ArgumentNullException(nameof(childDto));
        }

        if (childDto.DateOfBirth > DateTime.Now)
        {
            throw new ArgumentException(
                $"{nameof(ChildDto.DateOfBirth)}: {childDto.DateOfBirth} is bigger than current date.");
        }
    }

    private void ValidateOffsetFilter(OffsetFilter offsetFilter) =>
        ModelValidationHelper.ValidateOffsetFilter(offsetFilter);

    private void ValidateUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException($"The {nameof(userId)} parameter cannot be null, empty or white space.");
        }
    }

    private void ValidateId(long id)
    {
        if (id < 1)
        {
            throw new ArgumentException($"The {nameof(id)} parameter has to be greater than zero.");
        }
    }

    private async Task UpdateSocialGroups(Child child, ICollection<SocialGroupDto> socialGroupDtos)
    {
        if (socialGroupDtos.Any())
        {
            var socialGroupIds = socialGroupDtos.Select(x => x.Id).Distinct().ToArray();
            if (!new HashSet<long>(child.SocialGroups.Select(x => x.Id)).SetEquals(socialGroupIds))
            {
                var socialGroups = (await socialGroupRepository
                    .GetByFilter(x => socialGroupIds.Contains(x.Id))).ToList();
                if (socialGroupIds.Length != socialGroups.Count)
                {
                    throw new ArgumentException(@"Social groups contains some incorrect values", nameof(socialGroups));
                }

                child.SocialGroups = socialGroups;
            }
        }
        else
        {
            if (child.SocialGroups.Any())
            {
                child.SocialGroups = new List<SocialGroup>();
            }
        }
    }

    private async Task CompleteChildChangesAsync()
    {
        try
        {
            await childRepository.UnitOfWork.CompleteAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Updating a child failed");
            throw;
        }
    }
}