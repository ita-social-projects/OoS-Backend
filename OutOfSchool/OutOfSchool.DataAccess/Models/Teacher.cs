using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models
{
    public class Teacher : IKeyedEntity<Guid>, IImageDependentEntity<Teacher>
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public Gender Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Description { get; set; }

        public string CoverImageId { get; set; }

        public virtual List<Image<Teacher>> Images { get; set; }

        public Guid WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }
    }
}