using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Util
{
    public static class PictureControllerHelper
    {
        public static IActionResult ToFileResult(this PictureStorageModel pictureData)
        {
            return new FileStreamResult(pictureData.ContentStream, pictureData.ContentType);
        }
    }
}
