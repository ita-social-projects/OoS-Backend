using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models
{
    public class DirectionOfEducation
    {
        public long DirectionOfEducationId { get; set; }
        public string Title { get; set; }
        public List<ProfileOfEducation> Profiles { get; set; }
    }
}