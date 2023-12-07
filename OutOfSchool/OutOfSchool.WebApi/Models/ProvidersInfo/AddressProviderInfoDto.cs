﻿using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.ProvidersInfo;

public class AddressProviderInfoDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Street is required")]
    [MaxLength(60)]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Building number is required")]
    [MaxLength(15)]
    public string BuildingNumber { get; set; } = string.Empty;

    public CodeficatorAddressProviderInfoDto CodeficatorAddressDto { get; set; }
}
