using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class Subsubcategory
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public virtual List<Subsubcategory> Subcategories { get; set; }
    }
}