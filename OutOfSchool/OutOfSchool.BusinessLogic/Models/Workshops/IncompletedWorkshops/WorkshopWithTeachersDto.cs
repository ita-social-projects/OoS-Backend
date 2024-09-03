namespace OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;
public class WorkshopWithTeachersDto : WorkshopWithContactsDto
{
    public List<TeacherDTO> Teachers { get; set; }
}
