using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common.Exceptions
{
    [Serializable]
    public class ProviderNotFoundException : Exception
    {
        public ProviderNotFoundException()
        {
        }

        public ProviderNotFoundException(string message)
            : base(message)
        {
        }

        public ProviderNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ProviderNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
