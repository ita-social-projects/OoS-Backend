using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Pictures
{
    public interface IPictureValidatorService<TEntityConf>
    {
        bool ValidateImageSize(long size);

        bool ValidateImageResolution(int width, int height);

        bool ValidateImageFormat(string format);
    }
}
