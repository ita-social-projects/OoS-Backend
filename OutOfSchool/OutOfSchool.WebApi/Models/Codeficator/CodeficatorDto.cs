using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.Codeficator
{
    public class CodeficatorDto
    {
        public long Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        public long? ParentId { get; set; }

        [MaxLength(3)]
        public string Category { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
