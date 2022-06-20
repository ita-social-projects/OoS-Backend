using System;

namespace OutOfSchool.WebApi.Common
{
    public class WorkshopUpdateStatusException : Exception
    {
        public WorkshopUpdateStatusException(string message)
            : base(message) { }
    }
}