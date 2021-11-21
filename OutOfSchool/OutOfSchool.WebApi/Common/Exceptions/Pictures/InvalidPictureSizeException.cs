using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Exceptions.Pictures
{
    [Serializable]
    public class InvalidPictureSizeException : Exception
    {
        public InvalidPictureSizeException()
        {
        }

        public InvalidPictureSizeException(string message)
            : base(message)
        {
        }

        public InvalidPictureSizeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidPictureSizeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
