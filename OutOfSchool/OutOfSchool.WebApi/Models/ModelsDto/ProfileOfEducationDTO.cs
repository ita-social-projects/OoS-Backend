namespace OutOfSchool.WebApi.Models.ModelsDto
{
    public class ProfileOfEducationDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public virtual DirectionOfEducationDTO DirectionOfEducation { get; set; }
    }
}