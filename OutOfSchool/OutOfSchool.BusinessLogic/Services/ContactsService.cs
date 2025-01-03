using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Services;

public class ContactsService<TEntity, TDto> : IContactsService<TEntity, TDto>
    where TEntity : BusinessEntity, IHasContacts
    where TDto : IHasContactsDto<TEntity>
{
    private readonly IMapper mapper;

    public ContactsService(IMapper mapper)
    {
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public void ProcessCreate(TEntity existingEntity, TDto dto)
    {
        existingEntity.Contacts = mapper.Map<List<Contacts>>(dto.Contacts);
    }

    public void ProcessUpdate([NotNull] TEntity existingEntity, [NotNull] TDto dto)
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

        UpdateContactsInfo(
            existingEntity.Contacts,
            dto.Contacts,
            (existing, updated) =>
                existing.Title == updated.Title &&
                existing.Address.CATOTTGId == updated.Address.CATOTTGId &&
                string.Equals(existing.Address.Street, updated.Address.Street, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.Address.BuildingNumber, updated.Address.BuildingNumber,
                    StringComparison.OrdinalIgnoreCase),
            (existing, updated) =>
            {
                existing.Title = updated.Title;
                mapper.Map(updated.Address, existing.Address);

                this.SubUpdateContacts(existing, updated);
            }
        );
    }

    private void UpdateContactsInfo<TContactEntity, TContactDto>(
        List<TContactEntity> existingInfo,
        List<TContactDto> newInfo,
        Func<TContactEntity, TContactDto, bool> comparer,
        Action<TContactEntity, TContactDto> updateAction = null)
    {
        existingInfo.RemoveAll(e => !newInfo.Any(n =>
            comparer.Invoke(e, n)
        ));

        foreach (var info in newInfo)
        {
            var existing = existingInfo.FirstOrDefault(e =>
                comparer.Invoke(e, info));

            if (existing != null)
            {
                if (updateAction != null)
                {
                    updateAction.Invoke(existing, info);
                }
                else
                {
                    mapper.Map(info, existing);
                }
            }
            else
            {
                existingInfo.Add(mapper.Map<TContactEntity>(info));
            }
        }
    }

    private void SubUpdateContacts(Contacts entity, ContactsDto dto)
    {
        this.UpdateContactsInfo(
            entity.Phones,
            dto.Phones,
            (existing, updated) =>
                existing.Type == updated.Type && existing.Number == updated.Number);

        this.UpdateContactsInfo(
            entity.Emails,
            dto.Emails,
            (existing, updated) =>
                existing.Type == updated.Type &&
                string.Equals(existing.Address, updated.Address, StringComparison.OrdinalIgnoreCase));

        this.UpdateContactsInfo(
            entity.SocialNetworks,
            dto.SocialNetworks,
            (existing, updated) =>
                existing.Type == updated.Type &&
                string.Equals(existing.Url, updated.Url, StringComparison.OrdinalIgnoreCase));
    }
}