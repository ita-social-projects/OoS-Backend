#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OutOfSchool.Services.Models
{
    public class Section
    {
        public long SectionId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Group title is required")]
        public string Title { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"((\+)?\b(38)?(0[\d]{2}))([\d-]{7})",
            ErrorMessage = "Phone number format is incorrect. Example: XXX-XXX-XX-XX")]
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

        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public int DaysPerWeek { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        
        public bool WithDisabilityOptions { get; set; }
        
        [DataType(DataType.MultilineText)]
        public string? DisabilityOptionsDesc { get; set; }
        
        public string? Image { get; set; }

        public Address Address { get; set; }
        public DirectionOfEducation DirectionOfEducation { get; set; }
        public List<Teacher> Teachers { get; set; }
    }
}