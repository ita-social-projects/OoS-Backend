using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Workshops.Drafts;
public class WorkshopContactsDto : WorkshopDescriptionDto
{
    [Required]
    public long AddressId { get; set; }

    [Required]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public AddressDto Address { get; set; }
}
