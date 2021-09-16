using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Exceptions
{
    [Serializable]
    public class PictureNotFoundException : Exception
    {
        public PictureNotFoundException()
        {
        }

        public PictureNotFoundException(string message)
            : base(message)
        {
        }

        public PictureNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PictureNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
