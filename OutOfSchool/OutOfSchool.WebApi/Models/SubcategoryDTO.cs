namespace OutOfSchool.WebApi.Models
{
    public class SubcategoryDTO
    {
        public long Id { get; set; }
        
        public string Title { get; set; } = string.Empty;
        
        public virtual CategoryDTO Category { get; set; }
    }
}