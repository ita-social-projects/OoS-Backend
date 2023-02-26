using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class AverageRatingDto
{
    public float Rate { get; set; }

    public long RateQuantity { get; set; }

    public Guid EntityId { get; set; }
}