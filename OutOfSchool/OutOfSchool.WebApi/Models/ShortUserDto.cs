using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models;

public class ShortUserDto : BaseUserDto
{
    [DataType(DataType.EmailAddress)]
    public string UserName { get; set; }

    public string Role { get; set; }

    public bool IsRegistered { get; set; }
}