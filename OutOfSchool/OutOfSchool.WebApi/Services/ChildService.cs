using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;

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
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildService"/> class.
        /// </summary>
        /// <param name="childRepository">Repository for the Child entity.</param>
        /// <param name="parentRepository">Repository for the Parent entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ChildService(IEntityRepository<Child> childRepository, IParentRepository parentRepository, ILogger<ChildService> logger, IStringLocalizer<SharedResource> localizer)
        {
            this.childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
            this.parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        /// <inheritdoc/>
        public async Task<ChildDto> CreateChildForUser(ChildDto childDto, string userId)
        {
            logger.LogDebug($"Started creation of a new child with {nameof(Child.ParentId)}:{childDto.ParentId}, {nameof(userId)}:{userId}.");

            this.ValidateChildDto(childDto);

            if (userId is null)
            {
                throw new ArgumentNullException($"{nameof(userId)}");
            }

            var parent = (await parentRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false)).SingleOrDefault()
                ?? throw new ArgumentException($"There is no Parent with {nameof(userId)}:{userId}", userId);

            if (childDto.ParentId != parent.Id)
            {
                logger.LogWarning($"Prevented action! User:{userId} with {nameof(Child.ParentId)}:{parent.Id} was trying to create a new child with not his own {nameof(Child.ParentId)}:{childDto.ParentId}.");
                childDto.ParentId = parent.Id;
            }

            childDto.Id = 0;

            var newChild = await childRepository.Create(childDto.ToDomain()).ConfigureAwait(false);

            logger.LogDebug($"Child with Id:{newChild?.Id} ({nameof(Child.ParentId)}:{newChild.ParentId}, {nameof(userId)}:{userId}) was created successfully.");

            return newChild.ToModel();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ChildDto>> GetAllWithOffsetFilterOrderedById(OffsetFilter offsetFilter)
        {
            logger.LogDebug($"Getting all Children started. Amount of children to take: {offsetFilter.Size}, skip first: {offsetFilter.From}.");

            if (offsetFilter is null)
            {
                throw new ArgumentNullException($"{nameof(offsetFilter)}");
            }

            var totalAmount = await childRepository.Count().ConfigureAwait(false);

            var children = await childRepository.Get<long>(offsetFilter.From, offsetFilter.Size, $"{nameof(Child.SocialGroup)}", null, x => x.Id, true)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogDebug(!children.Any()
                ? $"There is no child in the Childrens table. Skipped records: {offsetFilter.From}. Order: by Child.Id."
                : $"{children.Count} children from {totalAmount} were successfully received. Skipped records: {offsetFilter.From}. Order: by Child.Id.");

            var searchResult = new SearchResult<ChildDto>()
            {
                TotalAmount = totalAmount,
                Entities = children.Select(x => x.ToModel()).ToList(),
            };

            return searchResult;
        }

        /// <inheritdoc/>
        public async Task<ChildDto> GetByIdAndUserId(long id, string userId)
        {
            logger.LogDebug($"User:{userId} is trying to get the child with id: {id}.");

            if (userId is null)
            {
                throw new ArgumentNullException($"{nameof(userId)}");
            }

            var child = id > 0
                ? (await childRepository.GetByFilter(child => child.Id == id, $"{nameof(Child.Parent)}").ConfigureAwait(false)).SingleOrDefault()
                : null;

            if (child is null)
            {
                logger.LogWarning($"User:{userId} is trying to get an unexisting child with id: {id}.");
                return null;
            }

            if (child.Parent.UserId != userId)
            {
                logger.LogWarning($"User{userId} is trying to get not his/her own child with id: {id}.");
                return null;
            }

            logger.LogDebug($"User:{userId} successfully got the child with id: {id}.");

            return child.ToModel();
        }

        /// <inheritdoc/>
        public async Task<SearchResult<ChildDto>> GetByParentIdOrderedByFirstName(long parentId, OffsetFilter offsetFilter)
        {
            if (offsetFilter is null)
            {
                throw new ArgumentNullException($"{nameof(offsetFilter)}");
            }

            logger.LogDebug($"Getting Children with ParentId: {parentId} started. Amount of children to take: {offsetFilter.Size}, skip first: {offsetFilter.From}.");

            var totalAmount = parentId > 0
                ? await childRepository.Count(x => x.ParentId == parentId).ConfigureAwait(false)
                : 0;

            var children = parentId > 0
                ? await childRepository.Get<string>(offsetFilter.From, offsetFilter.Size, "SocialGroup", x => x.ParentId == parentId, x => x.FirstName, true)
                    .ToListAsync()
                    .ConfigureAwait(false)
                : new List<Child>();

            logger.LogDebug(!children.Any()
                ? $"There is no child with ParentId: {parentId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
                : $"{children.Count} children with ParentId: {parentId} were successfully received. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

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
            if (userId is null)
            {
                throw new ArgumentNullException($"{nameof(userId)}");
            }

            if (offsetFilter is null)
            {
                throw new ArgumentNullException($"{nameof(offsetFilter)}");
            }

            logger.LogDebug($"Getting Child's for User started. Looking UserId = {userId}.");

            var totalAmount = await childRepository.Count(x => x.Parent.UserId == userId).ConfigureAwait(false);

            var children = await childRepository.Get<string>(offsetFilter.From, offsetFilter.Size, string.Empty, x => x.Parent.UserId == userId, x => x.FirstName, true)
                .ToListAsync()
                .ConfigureAwait(false);

            logger.LogDebug(!children.Any()
                ? $"There is no child for User:{userId}. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}."
                : $"{children.Count} records for User:{userId} were successfully received from the Children table. Skipped records: {offsetFilter.From}. Order: by {nameof(Child.FirstName)}.");

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
            logger.LogDebug($"Updating the child with Id: {childDto.Id} and {nameof(userId)}: {userId} started.");

            this.ValidateChildDto(childDto);

            if (userId is null)
            {
                throw new ArgumentNullException($"{nameof(userId)}");
            }

            try
            {
                var child = childDto.Id > 0
                    ? await childRepository.GetByFilterNoTracking(c => c.Id == childDto.Id, $"{nameof(Child.Parent)}").SingleOrDefaultAsync().ConfigureAwait(false)
                    : null;

                if (child is null)
                {
                    throw new ArgumentException($"User: {userId} is trying to update not existing Child (Id = {childDto.Id}).");
                }

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

                logger.LogDebug($"Child with Id = {updatedChild?.Id} was updated succesfully.");

                return updatedChild.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                // TODO: provide proper exception handling or remove this try-catch
                logger.LogError($"Updating failed. Children with Id = {childDto.Id} doesn't exist in the system.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteChildCheckingItsUserIdProperty(long id, string userId)
        {
            logger.LogDebug($"Deleting the child with Id: {id} and {nameof(userId)}: {userId} started.");

            if (userId is null)
            {
                throw new ArgumentNullException($"{nameof(userId)}");
            }

            try
            {
                var child = id > 0
                    ? await childRepository.GetByFilterNoTracking(c => c.Id == id, $"{nameof(Child.Parent)}").SingleOrDefaultAsync().ConfigureAwait(false)
                    : null;

                if (child is null)
                {
                    throw new ArgumentException($"User: {userId} is trying to delete not existing Child (Id = {id}).");
                }

                if (child.Parent.UserId != userId)
                {
                    throw new UnauthorizedAccessException($"User: {userId} is not authorized to delete not his own child. Child Id = {id}");
                }

                await childRepository.Delete(child).ConfigureAwait(false);

                logger.LogDebug($"Child with Id = {id} succesfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                // TODO: provide proper exception handling or remove this try-catch
                logger.LogError($"Deleting failed. Children with Id = {id} doesn't exist in the system.");
                throw;
            }
        }

        private void ValidateChildDto(ChildDto childDto)
        {
            if (childDto == null)
            {
                logger.LogInformation("Child creating failed. Child is null.");
                throw new ArgumentNullException(nameof(childDto), localizer["Child is null."]);
            }

            var isValid = true;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Validation of {nameof(ChildDto)} faild.");

            if (childDto.DateOfBirth > DateTime.Now)
            {
                isValid = false;
                stringBuilder.AppendLine($"{nameof(ChildDto.DateOfBirth)}: {childDto.DateOfBirth} is bigger than current date.");
            }

            if (childDto.FirstName.Length == 0)
            {
                isValid = false;
                stringBuilder.AppendLine($"{nameof(ChildDto.FirstName)}: is empty.");
            }

            if (childDto.LastName.Length == 0)
            {
                isValid = false;
                stringBuilder.AppendLine($"{nameof(ChildDto.LastName)}: is empty.");
            }

            if (!isValid)
            {
                throw new ArgumentException(stringBuilder.ToString(), nameof(childDto));
            }
        }
    }
}