using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Exceptions.Pictures
{
    [Serializable]
    public class InvalidPictureFormatException : Exception
    {
        public InvalidPictureFormatException()
        {
        }

        public InvalidPictureFormatException(string message)
            : base(message)
        {
        }

        public InvalidPictureFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidPictureFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
