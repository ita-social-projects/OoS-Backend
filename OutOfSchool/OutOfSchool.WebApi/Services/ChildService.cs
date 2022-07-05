﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Child entity.
    /// </summary>
    public class ChildService : IChildService
    {
        private readonly IEntityRepository<Child> childRepository;
        private readonly IParentRepository parentRepository;
        private readonly ILogger<ChildService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="childRepository">Repository for the Child entity.</param>
        /// <param name="parentRepository">Repository for the Parent entity.</param>
        /// <param name="logger">Logger.</param>
        public ChildService(IEntityRepository<Child> childRepository, IParentRepository parentRepository, ILogger<ChildService> logger)
        {
            this.childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            this.parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ChildDto> CreateChildForUser(ChildDto childDto, string userId)
        {
            ValidateChildDto(childDto);
            ValidateUserId(userId);

            logger.LogDebug($"Started creation of a new child with {nameof(Child.ParentId)}:{childDto.ParentId}, {nameof(userId)}:{userId}.");

            var parent = (await parentRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false)).SingleOrDefault()
                ?? throw new UnauthorizedAccessException($"Trying to create a new child the Parent with {nameof(userId)}:{userId} was not found.");

            if (childDto.ParentId != parent.Id)
            {
                logger.LogWarning($"Prevented action! User:{userId} with {nameof(Child.ParentId)}:{parent.Id} was trying to create a new child with not his own {nameof(Child.ParentId)}:{childDto.ParentId}.");
                childDto.ParentId = parent.Id;
            }

            childDto.Id = default;

            var newChild = await childRepository.Create(childDto.ToDomain()).ConfigureAwait(false);

            logger.LogDebug($"Child with Id:{newChild.Id} ({nameof(Child.ParentId)}:{newChild.ParentId}, {nameof(userId)}:{userId}) was created successfully.");

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ChildDto>> GetByFilter(SearchStringFilter filter)
        {
            filter ??= new SearchStringFilter();

            logger.LogDebug($"Getting all Children started. Amount of children to take: {filter.Size}, skip first: {filter.From}.");

            this.ValidateOffsetFilter(filter);

            var filterPredicate = PredicateBuild(filter);

            var totalAmount = await childRepository.Count(filterPredicate).ConfigureAwait(false);

            var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
                {
                    { x => x.Id, SortDirection.Ascending },
                };

            var children = await childRepository.Get(filter.From, filter.Size, $"{nameof(Child.SocialGroups)}", filterPredicate, sortExpression)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogDebug(children.Any()
                ? $"{children.Count} children from {totalAmount} were successfully received. Skipped records: {filter.From}. Order: by Child.Id."
                : $"There is no child in the Children table. Skipped records: {filter.From}. Order: by Child.Id.");

            var searchResult = new SearchResult<ChildDto>()
            {
                TotalAmount = totalAmount,
                Entities = children.Select(x => x.ToModel()).ToList(),
            };

            return searchResult;
        }

        /// <inheritdoc/>
        public async Task<ChildDto> GetByIdAndUserId(Guid id, string userId)
        {
            this.ValidateUserId(userId);

            logger.LogDebug($"User:{userId} is trying to get the child with id: {id}.");

            var child = (await childRepository.GetByFilter(child => child.Id == id, $"{nameof(Child.Parent)}").ConfigureAwait(false)).SingleOrDefault()
                ?? throw new UnauthorizedAccessException($"User:{userId} is trying to get an unexisting child with id: {id}.");

            if (child.Parent.UserId != userId)
            {
                throw new UnauthorizedAccessException($"User{userId} is trying to get not his/her own child with id: {id}.");
            }

            logger.LogDebug($"User:{userId} successfully got the child with id: {id}.");

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ChildDto>> GetByParentIdOrderedByFirstName(Guid parentId, OffsetFilter offsetFilter)
        {
            ValidateOffsetFilter(offsetFilter);

            logger.LogDebug($"Getting Children with ParentId: {parentId} started. Amount of children to take: {offsetFilter.Size}, skip first: {offsetFilter.From}.");

            var totalAmount = await childRepository.Count(x => x.ParentId == parentId).ConfigureAwait(false);

            var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
                {
                    { x => x.FirstName, SortDirection.Ascending },
                };

            var children = await childRepository
                .Get(offsetFilter.From, offsetFilter.Size, "SocialGroup", x => x.ParentId == parentId, sortExpression)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogDebug(children.Any()
                ? $"{children.Count} children with ParentId: {parentId} were successfully received. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
                : $"There is no child with ParentId: {parentId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

            var searchResult = new SearchResult<ChildDto>()
            {
                TotalAmount = totalAmount,
                Entities = children.Select(x => x.ToModel()).ToList(),
            };

            return searchResult;
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ChildDto>> GetByUserId(string userId, OffsetFilter offsetFilter)
        {
            this.ValidateUserId(userId);
            this.ValidateOffsetFilter(offsetFilter);

            logger.LogDebug($"Getting Child's for User started. Looking UserId = {userId}.");

            var totalAmount = await childRepository.Count(x => x.Parent.UserId == userId).ConfigureAwait(false);

            var sortExpression = new Dictionary<Expression<Func<Child, object>>, SortDirection>
                {
                    { x => x.FirstName, SortDirection.Ascending },
                };

            var children = await childRepository
                .Get(offsetFilter.From, offsetFilter.Size, string.Empty, x => x.Parent.UserId == userId, sortExpression)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogDebug(children.Any()
                ? $"{children.Count} children for User:{userId} were successfully received. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
                : $"There is no child for User:{userId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

            var searchResult = new SearchResult<ChildDto>()
            {
                TotalAmount = totalAmount,
                Entities = children.Select(x => x.ToModel()).ToList(),
            };

            return searchResult;
        }

        /// <inheritdoc/>
        public async Task<ChildDto> UpdateChildCheckingItsUserIdProperty(ChildDto childDto, string userId)
        {
            this.ValidateChildDto(childDto);
            this.ValidateUserId(userId);

            logger.LogDebug($"Updating the child with Id: {childDto.Id} and {nameof(userId)}: {userId} started.");

            var child = await childRepository.GetByFilterNoTracking(c => c.Id == childDto.Id, $"{nameof(Child.Parent)}")
                                             .SingleOrDefaultAsync()
                                             .ConfigureAwait(false)
                    ?? throw new UnauthorizedAccessException($"User: {userId} is trying to update not existing Child (Id = {childDto.Id}).");

            if (child.Parent.UserId != userId)
            {
                throw new UnauthorizedAccessException($"User: {userId} is trying to update not his own child. Child Id = {childDto.Id}");
            }

            if (childDto.ParentId != child.ParentId)
            {
                logger.LogWarning($"Prevented action! User:{userId} with {nameof(Child.ParentId)}:{child.ParentId} was trying to update his child with not his own {nameof(Child.ParentId)}:{childDto.ParentId}.");
                childDto.ParentId = child.ParentId;
            }

            var updatedChild = await childRepository.Update(childDto.ToDomain()).ConfigureAwait(false);

            logger.LogDebug($"Child with Id = {updatedChild.Id} was updated succesfully.");

            return updatedChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task DeleteChildCheckingItsUserIdProperty(Guid id, string userId)
        {
            this.ValidateUserId(userId);

            logger.LogDebug($"Deleting the child with Id: {id} and {nameof(userId)}: {userId} started.");

            var child = await childRepository.GetByFilterNoTracking(c => c.Id == id, $"{nameof(Child.Parent)}")
                                             .SingleOrDefaultAsync()
                                             .ConfigureAwait(false)
                ?? throw new UnauthorizedAccessException($"User: {userId} is trying to delete not existing Child (Id = {id}).");

            if (child.Parent.UserId != userId)
            {
                throw new UnauthorizedAccessException($"User: {userId} is not authorized to delete not his own child. Child Id = {id}");
            }

            await childRepository.Delete(child).ConfigureAwait(false);

            logger.LogDebug($"Child with Id = {id} succesfully deleted.");
        }

        private void ValidateChildDto(ChildDto childDto)
        {
            if (childDto == null)
            {
                throw new ArgumentNullException(nameof(childDto));
            }

            if (childDto.DateOfBirth > DateTime.Now)
            {
                throw new ArgumentException($"{nameof(ChildDto.DateOfBirth)}: {childDto.DateOfBirth} is bigger than current date.");
            }
        }

        private void ValidateOffsetFilter(OffsetFilter offsetFilter) => ModelValidationHelper.ValidateOffsetFilter(offsetFilter);

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

        private Expression<Func<Child, bool>> PredicateBuild(SearchStringFilter filter)
        {
            var predicate = PredicateBuilder.True<Child>();

            if (!string.IsNullOrWhiteSpace(filter.SearchString))
            {
                var tempPredicate = PredicateBuilder.False<Child>();

                foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
                {
                    tempPredicate = tempPredicate.Or(
                        x => x.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.Parent.User.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                            || x.Parent.User.PhoneNumber.Contains(word, StringComparison.InvariantCulture));
                }

                predicate = predicate.And(tempPredicate);
            }

            return predicate;
        }
    }
}
