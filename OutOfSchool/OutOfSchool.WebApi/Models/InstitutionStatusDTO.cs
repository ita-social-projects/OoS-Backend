using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class InstitutionStatusDTO
{
    public long Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}