using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.SocialGroup;

public class SocialGroupCreate
{
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;
}

public static class SocialGroupCreateExtensions
{
    public static OutOfSchool.Services.Models.SocialGroup ToModel(this SocialGroupCreate socialGroup)
        => new()
        {
            Id = socialGroup.Id,
            Name = socialGroup.Name,
        };
}