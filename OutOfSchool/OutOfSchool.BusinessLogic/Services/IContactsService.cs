using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines operations for creating and updating contacts on a business entity.
/// </summary>
public interface IContactsService<in TEntity, in TDto>
    where TEntity : BusinessEntity, IHasContacts
    where TDto : IHasContactsDto<TEntity>
{
    /// <summary>
    /// Populates new contacts for the specified entity based on the provided DTO.
    /// </summary>
    /// <param name="entity">
    /// The business entity (e.g., Workshop) to which the contacts will be added.
    /// </param>
    /// <param name="dto">
    /// The data transfer object containing the contact information to create.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the DTO contains more than one contact with <c>IsDefault = true</c>.
    /// </exception>
    void PrepareNewContacts(TEntity entity, TDto dto);
    
    /// <summary>
    /// Updates existing contacts for the specified entity based on the provided DTO.
    /// </summary>
    /// <param name="entity">
    /// The business entity (e.g., Workshop) whose contacts will be updated.
    /// </param>
    /// <param name="dto">
    /// The data transfer object containing updated contact information.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the DTO contains more than one contact with <c>IsDefault = true</c>.
    /// </exception>
    void PrepareUpdatedContacts(TEntity entity, TDto dto);
}