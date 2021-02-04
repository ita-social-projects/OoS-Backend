using System.Collections.Generic;

namespace OutOfSchool.Services.Models
{
    public class DirectionOfEducation
    {
        public long DirectionId { get; set; }
        public string Title { get; set; }
        public List<ProfileOfEducation> Profiles { get; set; }
    }
}