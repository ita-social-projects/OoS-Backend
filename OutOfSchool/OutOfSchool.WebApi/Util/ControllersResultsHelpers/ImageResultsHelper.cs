using System;
using System.Collections.Generic;
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
