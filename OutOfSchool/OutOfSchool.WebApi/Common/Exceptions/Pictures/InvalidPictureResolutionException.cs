using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Exceptions.Pictures
{
    public class InvalidPictureResolutionException : Exception
    {
        public InvalidPictureResolutionException()
        {
        }

        public InvalidPictureResolutionException(string message)
            : base(message)
        {
        }

        public InvalidPictureResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidPictureResolutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
