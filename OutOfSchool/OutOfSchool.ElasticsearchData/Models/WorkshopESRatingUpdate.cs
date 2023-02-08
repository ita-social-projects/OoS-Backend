namespace OutOfSchool.ElasticsearchData.Models;

public class WorkshopESRatingUpdate : IPartial<WorkshopES>
{
    public float Rating { get; set; }
}