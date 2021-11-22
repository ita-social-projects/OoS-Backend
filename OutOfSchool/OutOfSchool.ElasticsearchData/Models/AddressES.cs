﻿using Nest;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class AddressES
    {
        public long Id { get; set; }

        public string Region { get; set; }

        public string District { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string BuildingNumber { get; set; }

        [GeoPoint]
        public GeoLocation Point { get; set; }

    }
}
