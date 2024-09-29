using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Models;

public class UpdateEmployeeDto : AdminBaseDto
{
    [Required(ErrorMessage = "Id is required")]
    public string Id { get; set; }

    // to specify workshops, which can be managed by provider admin
    public List<Guid> ManagedWorkshopIds { get; set; }
}
