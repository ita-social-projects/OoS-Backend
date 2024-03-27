namespace OutOfSchool.WebApi.Models;

public interface IHasImages
{
    IList<string> ImageIds { get; set; }

    List<IFormFile> ImageFiles { get; set; }
}