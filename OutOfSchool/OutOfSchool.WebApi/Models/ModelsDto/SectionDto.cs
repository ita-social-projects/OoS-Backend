namespace OutOfSchool.WebApi.Models.ModelsDto
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class SectionDto
    {
        public long Id { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Group title is required")]
        public string Title { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"((\+)?\b(38)?(0[\d]{2}))([\d-]{7})",
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [RegularExpression(@"(^\d+(,\d{1,2})?$)")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public bool WithDisabilityOptions { get; set; }

        [DataType(DataType.MultilineText)]
        public string? DisabilityOptionsDesc { get; set; }

        public bool AddressSameAsOrganization { get; set; }

        public AddressDto Address { get; set; }

        public string? Image { get; set; }
    }
}