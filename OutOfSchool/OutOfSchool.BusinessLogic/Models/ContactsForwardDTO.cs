namespace OutOfSchool.BusinessLogic.Models;

public class ContactsForwardDTO
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }
    public string AddressType { get; set; }
    public Address Address { get; set; }
    public string Website { get; set; }
    public string Facebook { get; set; }
    public string Instagram { get; set; }
    public Contact Contact { get; set; }
}