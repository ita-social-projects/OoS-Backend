using System;
using System.Runtime.Serialization;

namespace OutOfSchool.WebApi.Common.Exceptions
{
    [Serializable]
    public class WorkshopNotFoundException : Exception
    {
        public WorkshopNotFoundException()
        {
        }

        public WorkshopNotFoundException(string message)
            : base(message)
        {
        }

        public WorkshopNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected WorkshopNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
