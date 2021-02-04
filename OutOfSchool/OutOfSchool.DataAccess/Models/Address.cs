namespace OutOfSchool.Services.Models

{
    public class Address
    {
        public long AddressId { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string BuildingNumb { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}