using System;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class TeacherES
    {
        public long Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public long WorkshopId { get; set; }
    }
}
