using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface IContactsForwardService
{
    //TODO: write description of these methods

    Task<IEnumerable<ContactsForwardDTO>> GetAll();

    Task<ContactsForwardDTO> GetById(long id);

    Task<ContactsForwardDTO> Create(ContactsForward dto);

    Task<ContactsForwardDTO> Update(ContactsForward dto);

    Task Delete(long id);
}