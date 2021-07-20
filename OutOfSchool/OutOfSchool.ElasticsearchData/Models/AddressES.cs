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

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
