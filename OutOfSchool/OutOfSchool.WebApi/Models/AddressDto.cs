namespace OutOfSchool.WebApi.Models
{
    public class AddressDTO
    {
        public long Id { get; set; }

        public string Region { get; set; }

        public string District { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string Building { get; set; }
    }
}