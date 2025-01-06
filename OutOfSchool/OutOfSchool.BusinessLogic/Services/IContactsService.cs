using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface IContactsService<in TEntity, in TDto>
    where TEntity : BusinessEntity, IHasContacts
    where TDto : IHasContactsDto<TEntity>
{
    void PrepareNewContacts(TEntity existingEntity, TDto dto);
    
    void PrepareUpdatedContacts(TEntity entity, TDto dto);
}