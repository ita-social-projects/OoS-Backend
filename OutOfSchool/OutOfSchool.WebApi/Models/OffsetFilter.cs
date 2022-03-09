using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models
{
    /// <summary>
    /// The filter to skip and take specified amount of entites from the collection.
    /// When Size and From are set with default values all entities in the collection will be taken.
    /// </summary>
    public class OffsetFilter
    {
        private const int MaxSize = 100;

        private int size = 10;

        /// <summary>
        /// Gets or sets the amount of entities to take from collection.
        /// </summary>
        public int Size
        {
            get => size;

            set => size = (value > MaxSize) ? MaxSize : value;
        }

        /// <summary>
        /// Gets or sets the amount of entities to skip before taking.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Field value should be in a range from 0 to 2 147 483 647")]
        public int From { get; set; } = 0;
    }
}
