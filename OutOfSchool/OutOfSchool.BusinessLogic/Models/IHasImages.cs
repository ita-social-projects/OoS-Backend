namespace OutOfSchool.BusinessLogic.Models;

public interface IHasImages
{
    IList<string> ImageIds { get; set; }

    List<IFormFile> ImageFiles { get; set; }
}