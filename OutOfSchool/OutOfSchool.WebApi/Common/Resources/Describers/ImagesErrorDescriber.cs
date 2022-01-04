using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.WebApi.Common.Resources.Describers
{
    public class ImagesErrorDescriber
    {
        public virtual OperationError DefaultError()
        {
            return new OperationError
            {
                Code = nameof(DefaultError),
                Description = ResourceInstances.ImageResource.DefaultError,
            };
        }

        public virtual OperationError UploadingError()
        {
            return new OperationError
            {
                Code = nameof(UploadingError),
                Description = ResourceInstances.ImageResource.UploadingError,
            };
        }

        public virtual OperationError RemovingError()
        {
            return new OperationError
            {
                Code = nameof(RemovingError),
                Description = ResourceInstances.ImageResource.RemovingError,
            };
        }

        public virtual OperationError ImageStorageError()
        {
            return new OperationError
            {
                Code = nameof(ImageStorageError),
                Description = ResourceInstances.ImageResource.ImageStorageError,
            };
        }

        public virtual OperationError ImageNotFoundError()
        {
            return new OperationError
            {
                Code = nameof(ImageNotFoundError),
                Description = ResourceInstances.ImageResource.ImageNotFoundError,
            };
        }

        public virtual OperationError UnexpectedValidationError()
        {
            return new OperationError
            {
                Code = nameof(UnexpectedValidationError),
                Description = ResourceInstances.ImageResource.UnexpectedValidationError,
            };
        }

        public virtual OperationError InvalidSizeError()
        {
            return new OperationError
            {
                Code = nameof(InvalidSizeError),
                Description = ResourceInstances.ImageResource.InvalidSizeError,
            };
        }

        public virtual OperationError InvalidFormatError()
        {
            return new OperationError
            {
                Code = nameof(InvalidFormatError),
                Description = ResourceInstances.ImageResource.InvalidFormatError,
            };
        }

        public virtual OperationError InvalidResolutionError()
        {
            return new OperationError
            {
                Code = nameof(InvalidResolutionError),
                Description = ResourceInstances.ImageResource.InvalidResolutionError,
            };
        }

        public virtual OperationError EntityNotFoundError()
        {
            return new OperationError
            {
                Code = nameof(EntityNotFoundError),
                Description = ResourceInstances.ImageResource.EntityNotFoundError,
            };
        }

        public virtual OperationError NoGivenImagesError()
        {
            return new OperationError
            {
                Code = nameof(NoGivenImagesError),
                Description = ResourceInstances.ImageResource.NoGivenImagesError,
            };
        }

        public virtual OperationError UpdateEntityError()
        {
            return new OperationError
            {
                Code = nameof(UpdateEntityError),
                Description = ResourceInstances.ImageResource.UpdateEntityError,
            };
        }

        public virtual OperationError ExceedingCountOfImagesError(int countOfImages)
        {
            return new OperationError
            {
                Code = nameof(ExceedingCountOfImagesError),
                Description = ResourceInstances.ImageResource.ExceedingCountOfImagesError(countOfImages),
            };
        }
    }
}
