namespace OutOfSchool.BusinessLogic.Models;

public class FullEmployeeDto : EmployeeDto
{
    public List<ShortEntityDto> WorkshopTitles { get; set; }
}
