namespace OutOfSchool.WebApi.Models
{
    public class ChangesLogFilter : OffsetFilter
    {
        public string Table { get; set; }

        public string Field { get; set; }

        public string RecordId { get; set; }
    }
}
