using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class Category
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public List<Subcategory> Subcategories { get; set; }
    }
}