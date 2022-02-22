using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models.Images
{
    public class DbImageContentTypeModel
    {
        public int Id { get; set; }

        public string ContentTypeValue { get; set; }

        public List<DbImageModel> Images { get; set; }
    }
}
