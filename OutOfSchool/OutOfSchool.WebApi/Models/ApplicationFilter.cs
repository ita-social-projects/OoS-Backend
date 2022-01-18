using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationFilter
    {
        [Range(1, 7, ErrorMessage = "Status filter should be from 1 to 7")]
        public ApplicationStatus Status { get; set; }

        public bool OrderByDateAscending { get; set; } = true;

        public bool OrderByAlphabetically { get; set; } = true;

        public bool OrderByStatus { get; set; } = true;

        public IEnumerable<Guid> Workshops { get; set; } = null;
    }
}
