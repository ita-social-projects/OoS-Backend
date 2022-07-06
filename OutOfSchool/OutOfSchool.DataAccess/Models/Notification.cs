﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class Notification : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; }

    public Dictionary<string, string> Data { get; set; }

    public string GroupedData { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    public NotificationAction Action { get; set; }

    [Required]
    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? ReadDateTime { get; set; }

    public Guid? ObjectId { get; set; }
}