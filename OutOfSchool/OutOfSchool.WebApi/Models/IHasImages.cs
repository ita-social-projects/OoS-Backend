namespace OutOfSchool.WebApi.Models;

public interface IHasImages
{
    IList<string> ImageIds { get; }

    List<IFormFile> ImageFiles { get; }
}