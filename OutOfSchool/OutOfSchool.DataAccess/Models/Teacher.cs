using System;

namespace OutOfSchool.Services.Models
{
    public class Teacher
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public Guid WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }
    }
}