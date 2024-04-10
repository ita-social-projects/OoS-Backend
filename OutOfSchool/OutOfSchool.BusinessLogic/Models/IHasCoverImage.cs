using Microsoft.AspNetCore.Http;

namespace OutOfSchool.BusinessLogic.Models;

public interface IHasCoverImage
{
    IFormFile CoverImage { get; }

    string CoverImageId { get; }
}
