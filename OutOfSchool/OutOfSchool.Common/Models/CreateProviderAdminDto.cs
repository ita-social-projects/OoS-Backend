using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Models;

public class CreateProviderAdminDto : AdminBaseDto
{
    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    public string ReturnUrl { get; set; }

    public Guid ProviderId { get; set; }

    public string UserId { get; set; }

    // to specify if its assistant or deputy
    public bool IsDeputy { get; set; }

    // to specify workshops, which can be managed by provider admin
    public List<Guid> ManagedWorkshopIds { get; set; }
}