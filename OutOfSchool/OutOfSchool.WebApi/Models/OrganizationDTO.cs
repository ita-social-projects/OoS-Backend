using System.ComponentModel.DataAnnotations;
using AutoMapper;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Models
{
    public class OrganizationDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [DataType(DataType.Text)]
        public string Title { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"([\d]{9})",
            ErrorMessage = "Phone number format is incorrect. Example: 380 50-123-45-67")]
        [DisplayFormat(DataFormatString = "{0:+38 XXX-XXX-XX-XX}")]
        public string Phone { get; set; }

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
        [RegularExpression(@"(^[0-9]{1,6}$)", ErrorMessage = "Invalid MFO format. Example: XXX XXX")]
        [StringLength(6, ErrorMessage = "MFO code must contain 6 digits")]
        public string MFO { get; set; }

        [Required(ErrorMessage = "EDRPOU code is required")]
        [RegularExpression(@"^(\d{8}|\d{10})$",
            ErrorMessage = "EDRPOU code must contain 8 or 10 digits")]
        public string EDRPOU { get; set; }

        [Required(ErrorMessage = "INPP code is required")]
        [RegularExpression(@"^(\d{9}|\d{10}|\d{12})$",
            ErrorMessage = "INPP code must contain 12, 10 or 9 digits")]
        public string INPP { get; set; }

        public string? Image { get; set; }

        public long UserId { get; set; }
        
        [Required(ErrorMessage = "Type of organization is required")] 
        public OrganizationType Type { get; set; }
    }
}