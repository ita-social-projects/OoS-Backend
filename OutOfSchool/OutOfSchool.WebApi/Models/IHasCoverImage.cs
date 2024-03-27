namespace OutOfSchool.WebApi.Models;

public interface IHasCoverImage
{
    IFormFile CoverImage { get; }

    string CoverImageId { get; }
}
