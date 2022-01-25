using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models
{
    public class Teacher : IKeyedEntity<Guid>, IImageDependentEntity<Teacher>
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Description { get; set; }

        public string AvatarImageId { get; set; }

        public Guid WorkshopId { get; set; }

        public virtual Workshop Workshop { get; set; }

        public virtual List<Image<Teacher>> Images { get; set; }
    }
}