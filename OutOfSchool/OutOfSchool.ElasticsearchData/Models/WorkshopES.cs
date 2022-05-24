using System;
using System.Collections.Generic;
using Nest;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopES
    {
        // TODO: check Nested attribute
        [Keyword]
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string CoverImageId { get; set; }

        public float Rating { get; set; }

        [Keyword]
        public Guid ProviderId { get; set; }

        public string ProviderTitle { get; set; }

        public InstitutionType ProviderInstitution { get; set; }

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

        [Nested]
        public List<DateTimeRangeES> DateTimeRanges { get; set; }

        public List<TeacherES> Teachers { get; set; }
    }
}