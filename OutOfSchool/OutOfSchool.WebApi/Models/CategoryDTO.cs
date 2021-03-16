using System.Collections.Generic;

namespace OutOfSchool.WebApi.Models
{
    public class CategoryDTO
    {
        public long Id { get; set; }

        public string Title { get; set; } = default;

        public virtual List<SubcategoryDTO> Subcategories { get; }
    }
}