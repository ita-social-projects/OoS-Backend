using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.SocialGroup;

public class SocialGroupCreate
{
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;
}
