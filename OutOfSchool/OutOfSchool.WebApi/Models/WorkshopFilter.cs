using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class WorkshopFilter
    {
        public string SearchFieldText { get; set; } = string.Empty;

        public int Age { get; set; } = 0;

        public int DaysPerWeek { get; set; } = 0;

        public bool Disability { get; set; } = true;

        public int MinPrice { get; set; } = 0;

        public int MaxPrice { get; set; } = 0;

        public bool OrderByPriceAscending { get; set; } = true;

        public IEnumerable<string> Directions { get; set; } = null;

        public IEnumerable<string> Departments { get; set; } = null;

        public IEnumerable<string> Classes { get; set; } = null;
    }
}
