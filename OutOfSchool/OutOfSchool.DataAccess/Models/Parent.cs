﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common;
using OutOfSchool.Services.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ChatWorkshop;

namespace OutOfSchool.Services.Models;

public class Parent : IKeyedEntity<Guid>, ISoftDeleted, IHasUser
{
    public Parent()
    {
        Children = new List<Child>();
    }

    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public virtual IReadOnlyCollection<Child> Children { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public Gender? Gender { get; set; }

    [Required]
    [Column(TypeName = ModelsConfigurationConstants.DateColumnType)]
    public DateTime? DateOfBirth { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }

    /// <summary>
    /// Creates draft parent, that is later will be edited through API.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="utcNow">Current UTC time.</param>
    /// <returns>New draft parent.</returns>
    public static Parent CreateDraft(string userId, DateTime utcNow) => new()
    {
        UserId = userId,
        Gender = Enums.Gender.Male,
        DateOfBirth = utcNow.AddYears(-Constants.AdultAge),
    };
}