using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Models;

public class Parent : IKeyedEntity<Guid>
{
    public Parent()
    {
        Children = new List<Child>();
    }

    public Guid Id { get; set; }

    public virtual IReadOnlyCollection<Child> Children { get; set; }

    [Required]
    public string UserId { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }
}