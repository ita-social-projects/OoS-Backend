using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.BusinessLogic.Models.Workshops.Drafts;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopCreateRequestDto : WorkshopContactsDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public TeacherCreateDto DefaultTeacher { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<TeacherCreateDto> Teachers { get; set; }

    public Guid? DefaultTeacherId { get; set; }
}