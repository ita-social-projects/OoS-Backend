using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json.Serialization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Pictures;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Extensions.Pictures;
using OutOfSchool.WebApi.Common.Exceptions.Pictures;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Pictures;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Pictures;

namespace OutOfSchool.WebApi.Services.Pictures
{
    public class PictureService : IPictureService
    {
        private readonly IPictureStorage pictureStorage;
        private readonly IWorkshopRepository workshopRepository;
        private readonly IPictureRepository pictureRepository;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<PictureService> logger;
        private readonly IMapper mapper;

        public PictureService(IPictureStorage pictureStorage, IWorkshopRepository workshopRepository, IPictureRepository pictureRepository, IServiceProvider serviceProvider,ILogger<PictureService> logger, IMapper mapper)
        {
            this.pictureStorage = pictureStorage; //naming
            this.workshopRepository = workshopRepository;
            this.pictureRepository = pictureRepository;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.mapper = mapper;
        }

        public async Task<PictureStorageModel> GetByIdAsync(Guid pictureId)
        {
            var pictureMetadata = await pictureRepository.GetMetadataById(pictureId).ConfigureAwait(false);
            ValidatePictureMetadata(pictureMetadata);

            return new PictureStorageModel
            {
                ContentStream = await pictureStorage.GetByIdAsync(pictureMetadata.StorageId).ConfigureAwait(false),
                ContentType = pictureMetadata.ContentType,
            };
        }

        public async Task<PictureOperationResult> UploadWorkshopPicture(Guid workshopId, PictureStorageModel pictureModel)// UploadPictureAndUpdateWorkshop
        {
            var validator = GetValidator<Workshop>();
            try
            {
                if (!validator.ValidateImageSize(pictureModel.ContentStream.Length)) // create 1 method of Validation
                {
                    throw new InvalidPictureSizeException();
                }

                using (var image = Image.FromStream(pictureModel.ContentStream)) // check using memory
                {
                    if (!validator.ValidateImageFormat(image.RawFormat.ToString())) // create 1 method of Validation
                    {
                        throw new InvalidPictureFormatException();
                    }

                    // TODO: validate by supported ratios
                    if (!validator.ValidateImageResolution(image.Width, image.Height)) // create 1 method of Validation
                    {
                        throw new InvalidPictureResolutionException();
                    }
                }

                var workShop = await workshopRepository.GetById(workshopId).ConfigureAwait(false); // get Workshop from parameters
                ValidateEntity(workShop); // move to up

                var pictureStorageId = await pictureStorage.UploadPictureAsync(pictureModel.ContentStream).ConfigureAwait(false);

                var pictureMetadata = new PictureMetadata
                {
                    ContentType = pictureModel.ContentType,
                    StorageId = pictureStorageId,
                };
                // check what will happen if image was uploaded and workshop wasn't uploaded
                // try to make 2 try catch
                workShop.WorkshopPictures.Add(new Picture<Workshop> {PictureMetadata = pictureMetadata});
                await workshopRepository.Update(workShop).ConfigureAwait(false);

                return PictureOperationResult.Success;
            }
            catch (EntityNotFoundException ex)
            {
                // TODO: use resources for all errors
                logger.LogError("");
                return PictureOperationResult.Failed(new PictureOperationError{Code = "entityNotFound", Description = "InvalidPictureSizeException" });
            }
            catch (PictureStorageException ex)
            {
                // TODO: use resources for all errors
                return PictureOperationResult.Failed(new PictureOperationError { Code = "PictureStorageException", Description = "InvalidPictureSizeException" });
            }
            catch (InvalidPictureSizeException ex)
            {
                // TODO: use resources for all errors
                return PictureOperationResult.Failed(new PictureOperationError{Code = "InvalidPictureSizeException", Description = "InvalidPictureSizeException" });
            }
            catch (InvalidPictureFormatException ex)
            {
                // TODO: use resources for all errors
                return PictureOperationResult.Failed(new PictureOperationError{Code = "InvalidPictureFormatException", Description = "InvalidPictureFormatException" });
            }
            catch (InvalidPictureResolutionException ex)
            {
                // TODO: use resources for all errors
                return PictureOperationResult.Failed(new PictureOperationError{Code = "InvalidPictureResolutionException", Description = "InvalidPictureResolutionException" });
            }
        }

        private static void ValidateEntity<T>(T entity)
        {
            if (entity == null)
            {
                throw new EntityNotFoundException($"{nameof(entity)}, type: {nameof(T)}");
            }
        }

        private static void ValidatePictureMetadata(PictureMetadata pictureMetadata)
        {
            if (pictureMetadata == null)
            {
                throw new PictureNotFoundException(nameof(pictureMetadata));
            }
        }

        private IPictureValidatorService<T> GetValidator<T>()
        {
            return (IPictureValidatorService<T>)serviceProvider.GetService(typeof(IPictureValidatorService<T>));
        }
    }
}
