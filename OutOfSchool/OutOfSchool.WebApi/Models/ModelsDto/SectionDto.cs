using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ModelsDto
{
    public class SectionDto
    {
        
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Group title is required")]
        public string Title { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"([0-9]{3})([-]?)([0-9]{3})([-]?)([0-9]{2})([-]?)([0-9]{2})",
            ErrorMessage = "Phone number format is incorrect. Example: 050-123-45-67")]
        [DisplayFormat(DataFormatString = "{0:+38 XXX-XXX-XX-XX}")]
        public string Phone { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [DataType(DataType.Url)] 
        public string? Website { get; set; }

        [DataType(DataType.Url)] 
        public string? Facebook { get; set; }

        [DataType(DataType.Url)] 
        public string? Istagram { get; set; }
        
        [Required]
        public int MinAge { get; set; }
        [Required]
        public int MaxAge { get; set; }
        [Required]
        public int DaysPerWeek { get; set; }

        public decimal? Cost { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        
        public bool WithDisabilityOptions { get; set; }
        
        [DataType(DataType.MultilineText)]
        public string? DisabilityOptionsDesc { get; set; }
        
        public byte[]? Image { get; set; }

    }
}