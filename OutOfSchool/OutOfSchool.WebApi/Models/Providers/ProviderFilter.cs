using System.Collections.Generic;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderFilter : SearchStringFilter
{
    public IReadOnlyCollection<ProviderStatus> Status { get; set; } = new List<ProviderStatus>();

    public IReadOnlyCollection<ProviderLicenseStatus> LicenseStatus { get; set; } = new List<ProviderLicenseStatus>();
}