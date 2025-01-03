using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Services;

public interface IContactsService<in TEntity, in TDto>
    where TEntity : BusinessEntity, IHasContacts
    where TDto : IHasContactsDto<TEntity>
{
    void ProcessCreate(TEntity existingEntity, TDto dto);
    
    void ProcessUpdate(TEntity entity, TDto dto);
}