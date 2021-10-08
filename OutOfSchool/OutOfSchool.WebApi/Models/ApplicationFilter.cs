using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationFilter
    {
        [Range(0, 5, ErrorMessage = "Status filter should be from 0 to 5")]
        public ApplicationStatus Status { get; set; } = 0;

        public bool OrderByDateAscending { get; set; } = true;

        public bool OrderByAlphabetically { get; set; } = true;

        public bool OrderByStatus { get; set; } = true;

        public IEnumerable<long> Workshops { get; set; } = null;
    }
}
