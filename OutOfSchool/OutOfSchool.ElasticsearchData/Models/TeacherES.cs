using System;
using Nest;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class TeacherES
    {
        [Keyword]
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        [Keyword]
        public Guid WorkshopId { get; set; }
    }
}
