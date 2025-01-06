using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Services;

public class ContactsService<TEntity, TDto>(IMapper mapper) : IContactsService<TEntity, TDto>
    where TEntity : BusinessEntity, IHasContacts
    where TDto : IHasContactsDto<TEntity>
{
    public void PrepareNewContacts(TEntity existingEntity, TDto dto)
    {
        existingEntity.Contacts = mapper.Map<List<Contacts>>(dto.Contacts);
    }

    public void PrepareUpdatedContacts([NotNull] TEntity existingEntity, [NotNull] TDto dto)
    {
        if (existingEntity.Contacts.IsNullOrEmpty())
        {
            existingEntity.Contacts = mapper.Map<List<Contacts>>(dto.Contacts);
            return;
        }

        // TODO: During transition leave it as optional and if it is empty - do nothing.
        if (dto.Contacts.IsNullOrEmpty())
        {
            return;
        }

        existingEntity.Contacts.RemoveAll(e => !dto.Contacts.Any(n =>
            n.ContentEquals(e)
        ));

        foreach (var contactDto in dto.Contacts)
        {
            var existingContact = existingEntity.Contacts.FirstOrDefault(e =>
                contactDto.ContentEquals(e));

            if (existingContact != null)
            {
                existingContact.Title = contactDto.Title;
                mapper.Map(contactDto.Address, existingContact.Address);

                this.UpdateContactsInfo(
                    existingContact.Phones,
                    contactDto.Phones);

                this.UpdateContactsInfo(
                    existingContact.Emails,
                    contactDto.Emails);

                this.UpdateContactsInfo(
                    existingContact.SocialNetworks,
                    contactDto.SocialNetworks);
            }
            else
            {
                existingEntity.Contacts.Add(mapper.Map<Contacts>(contactDto));
            }
        }
    }

    private void UpdateContactsInfo<TContactEntity, TContactDto>(
        List<TContactEntity> existingInfo,
        List<TContactDto> newInfo)
        where TContactDto : IContentComparable<TContactEntity>
    {
        existingInfo.RemoveAll(e => !newInfo.Any(n =>
            n.ContentEquals(e)
        ));

        foreach (var info in newInfo)
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
}