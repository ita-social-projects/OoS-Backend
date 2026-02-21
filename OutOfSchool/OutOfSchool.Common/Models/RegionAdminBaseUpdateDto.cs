
using System;

namespace OutOfSchool.Common.Models;
public class RegionAdminBaseUpdateDto : RegionAdminBaseDto
{
    // overriding the properties of the base class to avoid [Required] attribute of unnecessary properties
    public new Guid InstitutionId { get; set; }
    public new string FirstName { get; set; }
    public new string LastName { get; set; }

    public new long CATOTTGId { get; set; }
}
