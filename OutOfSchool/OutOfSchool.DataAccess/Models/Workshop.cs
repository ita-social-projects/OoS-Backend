using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore.Metadata.Internal;

using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Services.Models.Pictures;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Models
{
    public class Workshop
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Workshop title is required")]
        [MinLength(1)]
        [MaxLength(60)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(
            Constants.PhoneNumberRegexModel,
            ErrorMessage = Constants.PhoneErrorMessage)]
        [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
        [MaxLength(Constants.UnifiedPhoneLength)]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Website { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Facebook { get; set; } = string.Empty;

        [DataType(DataType.Url)]
        [MaxLength(Constants.UnifiedUrlLength)]
        public string Instagram { get; set; } = string.Empty;

        [Required(ErrorMessage = "Children's min age is required")]
        [Range(0, 18, ErrorMessage = "Min age should be a number from 0 to 18")]
        public int MinAge { get; set; }

        [Required(ErrorMessage = "Children's max age is required")]
        [Range(0, 18, ErrorMessage = "Max age should be a number from 0 to 18")]
        public int MaxAge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000, ErrorMessage = "Field value should be in a range from 1 to 10 000")]
        public decimal Price { get; set; } = default;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool WithDisabilityOptions { get; set; } = default;

        [MaxLength(200)]
        public string DisabilityOptionsDesc { get; set; } = string.Empty;

        [MaxLength(256)]
        public string Logo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Head's information is required")]
        [MaxLength(50, ErrorMessage = "Field should not be longer than 50 characters")]
        public string Head { get; set; } = string.Empty;

        [Required(ErrorMessage = "Head's date of birth is required")]
        [DataType(DataType.Date)]
        [Column(TypeName = "date")]
        public DateTime HeadDateOfBirth { get; set; }

        [Required]
        [MaxLength(60)]
        public string ProviderTitle { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Keywords { get; set; } = string.Empty;

        [Required]
        public bool IsPerMonth { get; set; }

        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public long AddressId { get; set; }

        [Required]
        public long DirectionId { get; set; }

        [Required]
        public long DepartmentId { get; set; }

        [Required]
        public long ClassId { get; set; }

        public virtual Provider Provider { get; set; }

        public virtual Direction Direction { get; set; }

        public virtual Address Address { get; set; }

        public virtual Class Class { get; set; }

        public virtual List<Teacher> Teachers { get; set; }

        public virtual List<Application> Applications { get; set; }

        public virtual List<DateTimeRange> DateTimeRanges { get; set; }

        // These properties are only for navigation EF Core.
        public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }

        public virtual ICollection<Picture<Workshop>> WorkshopPictures { get; set; }
    }
}