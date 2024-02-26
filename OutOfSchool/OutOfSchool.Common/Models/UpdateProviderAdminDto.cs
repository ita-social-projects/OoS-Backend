using System;
using System.Collections.Generic;

namespace OutOfSchool.Common.Models;

public class UpdateProviderAdminDto : AdminBaseDto
{
    // to specify workshops, which can be managed by provider admin
    public List<Guid> ManagedWorkshopIds { get; set; }
}
