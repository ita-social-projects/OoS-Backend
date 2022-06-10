using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationFilter : OffsetFilter
    {
        public ApplicationStatus Status { get; set; }

        public bool OrderByDateAscending { get; set; } = true;

        public bool OrderByAlphabetically { get; set; } = true;

        public bool OrderByStatus { get; set; } = true;

        public bool ShowBlocked { get; set; } = false;

        public IEnumerable<Guid> Workshops { get; set; } = null;
    }
}
