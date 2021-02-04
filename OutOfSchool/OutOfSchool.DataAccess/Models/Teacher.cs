using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OutOfSchool.Services.Models
{
    public class Teacher
    {
        public long TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Description { get; set; }
        public byte[]? Image { get; set; }
        public Group Group { get; set; }
    }
}