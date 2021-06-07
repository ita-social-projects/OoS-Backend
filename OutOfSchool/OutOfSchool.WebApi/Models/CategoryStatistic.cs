namespace OutOfSchool.WebApi.Models
{
    public class CategoryStatistic
    {
        public CategoryDTO Category { get; set; }

        public int WorkshopsCount { get; set; }

        public int ApplicationsCount { get; set; }
    }
}
