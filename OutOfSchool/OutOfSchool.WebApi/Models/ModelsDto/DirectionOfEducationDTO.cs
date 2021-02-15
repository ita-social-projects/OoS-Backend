using System.Collections.Generic;

namespace OutOfSchool.WebApi.Models.ModelsDto
{
    public class DirectionOfEducationDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public virtual List<ProfileOfEducationDTO> ProfilesOfEducation { get; set; }
    }
}