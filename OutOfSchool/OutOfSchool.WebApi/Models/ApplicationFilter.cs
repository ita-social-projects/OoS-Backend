using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class ApplicationFilter
    {
        public int Status { get; set; } = 0;

        public bool OrderByDateAscending { get; set; } = true;

        public bool OrderByAlphabetically { get; set; } = true;

        public bool OrderByStatus { get; set; } = true;

        public IEnumerable<long> Workshops { get; set; } = null;
    }
}
