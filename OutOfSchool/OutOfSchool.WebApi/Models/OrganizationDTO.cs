using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Models
{
    public class OrganizationDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? Website { get; set; }     
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string Description { get; set; }
        public string MFO { get; set; }
        public string EDRPOU { get; set; }
        public string INPP { get; set; }
        public byte[]? Image { get; set; }
        public OrganizationType Type { get; set; }
        public long UserId { get; set; }
    }
}
