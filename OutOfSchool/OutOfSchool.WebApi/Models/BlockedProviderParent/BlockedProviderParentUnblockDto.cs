using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models.BlockedProviderParent
{
    public class BlockedProviderParentUnblockDto
    {
        [Required]
        public Guid ParentId { get; set; }

        [Required]
        public Guid ProviderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTimeOffset DateTimeTo { get; set; }
    }
}
