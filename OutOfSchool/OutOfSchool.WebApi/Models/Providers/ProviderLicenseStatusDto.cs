﻿using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderLicenseStatusDto
{
    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    [EnumDataType(typeof(ProviderLicenseStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderLicenseStatus LicenseStatus { get; set; }
}