using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class WorkshopFilter
    {
        public string SearchFieldText { get; set; } = null;

        public int? Age { get; set; } = null;

        public int? DaysPerWeek { get; set; } = null;

        public bool? Disability { get; set; } = null;

        public int? MinPrice { get; set; } = null;

        public int? MaxPrice { get; set; } = null;

        public bool? OrderByPriceAscending { get; set; } = null;

        public IEnumerable<string> Categories { get; set; } = null;

        public IEnumerable<string> Subcategories { get; set; } = null;

        public IEnumerable<string> Subsubcategories { get; set; } = null;
    }
}
