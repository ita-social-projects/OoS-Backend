namespace OutOfSchool.WebApi.Models
{
    public class SubcategoryDTO
    {
        public long Id { get; set; }
        
        public string Title { get; set; }
        
        public virtual CategoryDTO Category { get; set; }
    }
}