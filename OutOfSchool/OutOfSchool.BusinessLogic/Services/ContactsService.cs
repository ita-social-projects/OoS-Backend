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
        var unique = dto.Contacts.Distinct().ToList();

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
        var unique = dto.Contacts.Distinct().ToList();
        
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

    private void UpdateContactsInfo<TContactEntity, TContactDto>(
        List<TContactEntity> existingInfo,
        List<TContactDto> newInfo)
        where TContactEntity : class
        where TContactDto : IContentComparable<TContactEntity>
    {
        // Within each contact sub entry we leave only unique entries
        var unique = newInfo.Distinct().ToList();

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
}