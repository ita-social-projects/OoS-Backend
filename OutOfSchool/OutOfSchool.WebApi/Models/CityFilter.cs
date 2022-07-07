using System;

namespace OutOfSchool.WebApi.Models;

public class CityFilter
{
    public decimal Latitude { get; set; } = default;

    public decimal Longitude { get; set; } = default;
}