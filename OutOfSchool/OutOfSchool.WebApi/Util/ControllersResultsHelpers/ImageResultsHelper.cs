using System;
using System.Collections.Generic;
using MongoDB.Bson.IO;
using OutOfSchool.WebApi.Common;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OutOfSchool.WebApi.Util.ControllersResultsHelpers
{
    public static class ImageResultsHelper
    {
        public static object CreateMultipleUploadingResult(this MultipleKeyValueOperationResult multipleResults)
        {
            _ = multipleResults ?? throw new ArgumentNullException(nameof(multipleResults));

            return new
            {
                AllImagesUploaded = multipleResults.Succeeded,
                GeneralMessage = multipleResults.GeneralResultMessage,
                HasResults = multipleResults.HasResults,
                Results = multipleResults.HasResults ? multipleResults.Results : null,
            };
        }

        public static object CreateMultipleRemovingResult(this MultipleKeyValueOperationResult multipleResults)
        {
            _ = multipleResults ?? throw new ArgumentNullException(nameof(multipleResults));

            return new
            {
                AllImagesRemoved = multipleResults.Succeeded,
                GeneralMessage = multipleResults.GeneralResultMessage,
                HasResults = multipleResults.HasResults,
                Results = multipleResults.HasResults ? multipleResults.Results : null,
            };
        }
    }
}
