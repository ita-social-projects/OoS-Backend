namespace OutOfSchool.BusinessLogic.Models.StudySubjects
{
    public class UpdateWorkshopAttachmentDto
    {
        public IEnumerable<Guid> WorkshopIdsToAttach { get; set; }
        public IEnumerable<Guid> WorkshopIdsToDetach { get; set; }
    }
}
