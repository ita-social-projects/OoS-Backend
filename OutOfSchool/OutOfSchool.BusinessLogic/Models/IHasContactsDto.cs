#nullable enable

using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models;

public interface IHasContactsDto<TEntity>: IDto<TEntity, Guid>
where TEntity: BusinessEntity, IHasContacts
{
    public List<ContactsDto> Contacts { get; set; }
}