using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Config.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public sealed class TeacherImagesInteractionService :
        ChangeableImagesInteractionService<Teacher, Guid>,
        ITeacherImagesInteractionService
    {
        public TeacherImagesInteractionService(IImageService imageService, ISensitiveEntityRepository<Teacher> repository, ILogger<TeacherImagesInteractionService> logger, IOptions<ImagesLimits<Teacher>> limits)
            : base(imageService, repository, limits.Value, logger)
        {
        }
    }
}
