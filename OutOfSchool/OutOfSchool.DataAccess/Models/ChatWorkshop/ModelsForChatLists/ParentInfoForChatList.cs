using System;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.ChatWorkshop.ModelsForChatLists;

public class ParentInfoForChatList
{
    [Column(TypeName = "UUID")]
    public Guid Id { get; set; }

    public string UserId { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string FirstName { get; set; }

    public Gender? Gender { get; set; }
}