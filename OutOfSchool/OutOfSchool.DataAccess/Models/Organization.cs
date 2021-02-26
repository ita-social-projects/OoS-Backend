#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models
{
    public class Organization
    {
        public long Id { get; set; }
       
        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        public string Title { get; set; }
        
        [DataType(DataType.Url)]
        public string? Website { get; set; }
        
        [DataType(DataType.Url)]
        public string? Facebook { get; set; }
        
        [DataType(DataType.Url)]
        public string? Instagram { get; set; }
        
        [MaxLength(750)]   
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "MFO code is required")]
        [MinLength(6, ErrorMessage = "MFO code should contain 6 digits")]
        public string MFO { get; set; }

        [Required(ErrorMessage = "EDRPOU code is required")]
        [RegularExpression(@"^(\d{8}|\d{10})$", 
            ErrorMessage = "EDRPOU code should contain 8 or 10 digits")]
        public string EDRPOU { get; set; }
        
        [Required(ErrorMessage = "INPP code is required")]
        [RegularExpression(@"^(\d{9}|\d{10}|\d{12})$",
            ErrorMessage = "INPP code should contain 12, 10 or 9 digits")]
        public string INPP { get; set; }
        
        //[DataType(DataType.ImageUrl)]
        //public byte[]? Image { get; set; }
        
        [Required]
        public OrganizationType Type { get; set; }
        public string UserId { get; set; } 
    }
}