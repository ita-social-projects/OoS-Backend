using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class OffsetFilter
    {
        [Range(1, long.MaxValue)]
        public int Size { get; set; } = 12;

        public int From { get; set; } = 0;
    }
}
