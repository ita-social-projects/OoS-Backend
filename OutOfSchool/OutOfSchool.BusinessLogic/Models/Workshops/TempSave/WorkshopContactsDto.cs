using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Workshops.TempSave;
public class WorkshopContactsDto : WorkshopDescriptionDto, IHasContactsDto<Workshop>
{
    [Required]
    public long AddressId { get; set; }

    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public AddressDto Address { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<ContactsDto> Contacts { get; set; }
}
