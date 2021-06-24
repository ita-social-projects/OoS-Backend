﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.Services.Models
{
    public class User : IdentityUser
    {
        [DataType(DataType.DateTime)]
        public DateTime CreatingTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastLogin { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }

        [Required(ErrorMessage = "FirstName is required")]
        public string FirstName { get; set; }

        public string Role { get; set; }

        public bool IsRegistered { get; set; }

        // These properties are only for navigation EF Core.
        public virtual ICollection<ChatRoom> ChatRooms { get; set; }

        public virtual List<ChatRoomUser> ChatRoomUsers { get; set; }

        public virtual ICollection<ChatMessage> ChatMessages { get; set; }
    }
}