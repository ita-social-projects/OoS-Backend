using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    public class OffsetFilter
    {
        [Range(1, int.MaxValue)]
        public int Size { get; set; } = 12;

        [Range(0, int.MaxValue)]
        public int From { get; set; } = 0;
    }
}
