using System;

namespace OutOfSchool.Common.Models;

public class AreaAdminBaseUpdateDto : AreaAdminBaseDto
{
    // override properties from base class that was declared with [Required] attribute
    public new string FirstName { get; set; }
    public new string LastName { get; set; }
    public new Guid InstitutionId { get; set; }
    public new long CATOTTGId { get; set; }
}