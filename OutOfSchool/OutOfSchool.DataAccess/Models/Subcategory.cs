using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Subcategory
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        [MaxLength(60)]
        [MinLength(1)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public long CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public virtual List<Subsubcategory> Subsubcategories { get; set; }
    }
}
