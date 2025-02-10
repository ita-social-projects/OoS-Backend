using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Services;

/// <inheritdoc/>
public class ContactsService<TEntity, TDto>(IMapper mapper) : IContactsService<TEntity, TDto>
    where TEntity : BusinessEntity, IHasContacts
    where TDto : IHasContactsDto<TEntity>
{
    /// <inheritdoc/>
    public void PrepareNewContacts(TEntity entity, TDto dto)
    {
        // TODO: During transition leave it as optional and if it is empty - do nothing.
        if (dto.Contacts.IsNullOrEmpty())
        {
            return;
        }
        
        // here we check only top level contacts uniqueness
        var unique = dto.Contacts.Distinct(new ContactEqualityComparer<ContactsDto>()).ToList();

        ValidateContactsRequiredFields(unique);

        ValidateDefaultCount(unique);

        entity.Contacts = mapper.Map<List<Contacts>>(unique);
    }

    /// <inheritdoc/>
    public void PrepareUpdatedContacts([NotNull] TEntity entity, [NotNull] TDto dto)
    {
        // TODO: During transition leave it as optional and if it is empty - do nothing.
        if (dto.Contacts.IsNullOrEmpty())
        {
            return;
        }

        // here we check only top level contacts uniqueness
        var unique = dto.Contacts.Distinct(new ContactEqualityComparer<ContactsDto>()).ToList();

        ValidateContactsRequiredFields(unique);
        
        ValidateDefaultCount(unique);

        if (entity.Contacts.IsNullOrEmpty())
        {
            entity.Contacts = mapper.Map<List<Contacts>>(unique);
            return;
        }

        entity.Contacts.RemoveAll(e => !unique.Any(n =>
            n.ContentEquals(e)
        ));

        foreach (var contactDto in unique)
        {
            var existing = entity.Contacts.FirstOrDefault(e =>
                contactDto.ContentEquals(e));

            if (existing != null)
            {
                existing.Title = contactDto.Title;
                existing.IsDefault = contactDto.IsDefault;
                mapper.Map(contactDto.Address, existing.Address);

                this.UpdateContactsInfo(
                    existing.Phones,
                    contactDto.Phones);

                this.UpdateContactsInfo(
                    existing.Emails,
                    contactDto.Emails);

                this.UpdateContactsInfo(
                    existing.SocialNetworks,
                    contactDto.SocialNetworks);
            }
            else
            {
                entity.Contacts.Add(mapper.Map<Contacts>(contactDto));
            }
        }
    }

    /// <summary>
    /// Validates that required fields are present for each contact.
    /// </summary>
    /// <param name="contacts">The collection of contacts to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when address is null or phone numbers are missing.</exception>
    private static void ValidateContactsRequiredFields(IEnumerable<ContactsDto> contacts)
    {
        foreach (var contact in contacts)
        {
            if (contact.Address == null)
            {
                throw new InvalidOperationException("Address must be specified for each contact.");
            }

            if (contact.Phones.IsNullOrEmpty())
            {
                throw new InvalidOperationException("At least one phone number must be specified for each contact.");
            }
        }
    }

    /// <summary>
    /// Updates a list of contact information entities with new information, maintaining uniqueness.
    /// </summary>
    /// <typeparam name="TContactEntity">The type of the contact entity.</typeparam>
    /// <typeparam name="TContactDto">The type of the contact DTO.</typeparam>
    /// <param name="existingInfo">The existing list of contact information entities.</param>
    /// <param name="newInfo">The new list of contact information DTOs.</param>
    private void UpdateContactsInfo<TContactEntity, TContactDto>(
        List<TContactEntity> existingInfo,
        List<TContactDto> newInfo)
        where TContactEntity : class
        where TContactDto : IContentComparable<TContactEntity>, IEquatable<TContactDto>
    {
        // Within each contact sub entry we leave only unique entries
        var unique = newInfo.Distinct(new ContactEqualityComparer<TContactDto>()).ToList();

        existingInfo.RemoveAll(e => !unique.Any(n =>
            n.ContentEquals(e)
        ));

        foreach (var info in unique)
        {
            var existing = existingInfo.FirstOrDefault(e =>
                info.ContentEquals(e));

            if (existing != null)
            {
                mapper.Map(info, existing);
            }
            else
            {
                existingInfo.Add(mapper.Map<TContactEntity>(info));
            }
        }
    }

    /// <summary>
    /// Validates and ensures exactly one contact is marked as default.
    /// </summary>
    /// <param name="contactsDtos">The list of contacts to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when more than one contact is marked as default.</exception>
    private static void ValidateDefaultCount(List<ContactsDto> contactsDtos)
    {
        var defaultCount = contactsDtos.Count(c => c.IsDefault);
        switch (defaultCount)
        {
            case 0:
                // If no default, set the first contact to default
                contactsDtos[0].IsDefault = true;
                break;
            case > 1:
                throw new InvalidOperationException($"Exactly one Contact must be default, but found {defaultCount}.");
            case 1:
                // Exactly one is okay, do nothing special
                break;
        }
    }
    
    /// <summary>
    /// Compares objects of type T for equality, where T implements <see cref="IEquatable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    private sealed class ContactEqualityComparer<T> : IEqualityComparer<T>
    where T : IEquatable<T>
    {
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}