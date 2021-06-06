using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class CategoryStatistic
    {
        public CategoryDTO Category { get; set; }

        public int WorkshopsCount { get; set; }
    }
}
