﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.ChatWorkshop;

public class ChatRoomWorkshop : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    public bool IsBlocked { get; set; }

    public virtual Workshop Workshop { get; set; }

    public virtual Parent Parent { get; set; }

    public virtual ICollection<ChatMessageWorkshop> ChatMessages { get; set; }
}