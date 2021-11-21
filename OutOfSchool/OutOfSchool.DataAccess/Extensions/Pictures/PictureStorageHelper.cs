using System;
using System.Linq;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Pictures;

namespace OutOfSchool.Services.Extensions.Pictures
{
    public static class PictureStorageHelper
    {
        public static PictureMetadata GetPictureMetadata(this Workshop workshop, Guid pictureId) =>
            workshop.WorkshopPictures.FirstOrDefault(x => x.PictureMetadata.Id == pictureId)?.PictureMetadata;
    }
}
