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
        public string Image { get; set; }
        public Section Section { get; set; }
    }
}