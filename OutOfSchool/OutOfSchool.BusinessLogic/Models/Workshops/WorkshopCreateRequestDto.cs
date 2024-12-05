using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Workshops.Drafts;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopCreateRequestDto : WorkshopContactsDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public TeacherDTO DefaultTeacher { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<TeacherDTO> Teachers { get; set; }

    public Guid? DefaultTeacherId { get; set; }
}