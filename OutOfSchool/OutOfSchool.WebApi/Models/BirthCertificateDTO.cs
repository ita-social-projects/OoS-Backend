using System;

namespace OutOfSchool.WebApi.Models
{
    public class BirthCertificateDto
    {
        public long Id { get; set; }

        public string SvidSer { get; set; } = string.Empty;

        public string SvidNum { get; set; } = string.Empty;

        public string SvidNumMD5 { get; set; } = string.Empty;

        public string SvidWho { get; set; } = string.Empty;

        public DateTime SvidDate { get; set; } = default;

        public long ChildId { get; set; }
    }
}
