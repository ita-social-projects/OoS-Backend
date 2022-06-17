using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class ProviderLicenseStatusDto
    {
        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public ProviderLicenseStatus LicenseStatus { get; set; }
    }
}
