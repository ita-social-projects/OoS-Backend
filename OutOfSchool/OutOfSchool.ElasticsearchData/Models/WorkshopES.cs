using System.Collections.Generic;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopES
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Logo { get; set; }

        public float Rating { get; set; }

        public long ProviderId { get; set; }

        public string ProviderTitle { get; set; }

        public string Description { get; set; }

        public int MinAge { get; set; }

        public int MaxAge { get; set; }

        public decimal Price { get; set; }

        public bool IsPerMonth { get; set; }

        public long AddressId { get; set; }

        public AddressES Address { get; set; }

        public long DirectionId { get; set; }

        public string Direction { get; set; }

        public long DepartmentId { get; set; }

        public long ClassId { get; set; }

        public bool WithDisabilityOptions { get; set; }

        public string Keywords { get; set; }

        public IEnumerable<TeacherES> Teachers { get; set; }
    }
}