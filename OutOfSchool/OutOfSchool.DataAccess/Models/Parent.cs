using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models
{
    public class Parent
    {
        public Parent()
        {
            Children = new List<Child>();
        }

        public long Id { get; set; }

        public virtual IReadOnlyCollection<Child> Children { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual User User { get; set; }

        // These properties are only for navigation EF Core.
        public virtual ICollection<ChatRoom> ChatRooms { get; set; }
    }
}