using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class Category
    {
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<Subcategory> Subcategories { get; set; }
    }
}