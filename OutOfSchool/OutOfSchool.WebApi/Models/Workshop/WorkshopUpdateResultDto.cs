﻿using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Workshop
{
    public class WorkshopUpdateResultDto
    {
        public WorkshopDTO Workshop { get; set; }

        public OperationResult UploadingCoverImageResult { get; set; }

        public MultipleKeyValueOperationResult UploadingImagesResults { get; set; }
    }
}
