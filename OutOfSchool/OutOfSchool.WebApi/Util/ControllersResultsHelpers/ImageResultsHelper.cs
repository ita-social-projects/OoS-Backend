using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.IO;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OutOfSchool.WebApi.Util.ControllersResultsHelpers
{
    public static class ImageResultsHelper
    {
        public static MultipleImageUploadingResponse CreateMultipleUploadingResult(this MultipleKeyValueOperationResult multipleResults)
        {
            _ = multipleResults ?? throw new ArgumentNullException(nameof(multipleResults));

            return new MultipleImageUploadingResponse
            {
                AllImagesUploaded = multipleResults.Succeeded,
                GeneralMessage = multipleResults.GeneralResultMessage,
                HasResults = multipleResults.HasResults,
                Results = CreateResultsResponse(multipleResults),
            };
        }

        public static MultipleImageRemovingResponse CreateMultipleRemovingResult(this MultipleKeyValueOperationResult multipleResults)
        {
            _ = multipleResults ?? throw new ArgumentNullException(nameof(multipleResults));

            return new MultipleImageRemovingResponse
            {
                AllImagesRemoved = multipleResults.Succeeded,
                GeneralMessage = multipleResults.GeneralResultMessage,
                HasResults = multipleResults.HasResults,
                Results = CreateResultsResponse(multipleResults),
            };
        }

        public static SingleImageUploadingResponse CreateSingleUploadingResult(this OperationResult result)
        {
            _ = result ?? throw new ArgumentNullException(nameof(result));

            return new SingleImageUploadingResponse()
            {
                Result = result,
            };
        }

        public static SingleImageRemovingResponse CreateSingleRemovingResult(this OperationResult result)
        {
            _ = result ?? throw new ArgumentNullException(nameof(result));

            return new SingleImageRemovingResponse()
            {
                Result = result,
            };
        }

        private static IDictionary<short, OperationResult> CreateResultsResponse(MultipleKeyValueOperationResult multipleResults)
        {
            if (multipleResults.Succeeded)
            {
                return null;
            }

            return multipleResults.HasResults ? multipleResults.Results : null;
        }
    }
}
