namespace OutOfSchool.WebApi.Models
{
    public class ParentDtoWithContactInfo : ParentDTO
    {
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string FirstName { get; set; }
    }
}
