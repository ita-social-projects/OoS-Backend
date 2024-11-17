using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class Position : BusinessEntity
{
    public string Language { get; set; }

    public string Description { get; set; }

    public bool IsForRuralAreas { get; set; }

    public string Department { get; set; }

    public virtual Provider Provider { get; set; }

    public virtual Contact Contact { get; set; }

    [Required]
    public int SeatsAmount { get; set; }

    [Required]
    public string FullName { get; set; }

    public string ShortName { get; set; }

    [Required]
    public string GenitiveName { get; set; }

    public bool IsTeachingPosition { get; set; }

    [Required]
    public float Rate { get; set; }

    [Required]
    public float Tariff { get; set; }

    [Required]
    public string ClassifierType { get; set; }
}