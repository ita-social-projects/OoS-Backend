using System;

namespace OutOfSchool.WebApi.Models.Changes
{
    public class ProviderChangesLogDto : ChangesLogDtoBase
    {
        public Guid ProviderId { get; set; }

        public string ProviderTitle { get; set; }

        public string ProviderCity { get; set; }
    }
}
