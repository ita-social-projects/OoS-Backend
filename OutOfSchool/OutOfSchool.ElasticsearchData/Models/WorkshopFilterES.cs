using System.Collections.Generic;
using OutOfSchool.ElasticsearchData.Enums;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopFilterES
    {
        public List<long> Ids { get; set; } = null;

        public string SearchText { get; set; } = string.Empty;

        public OrderBy OrderByField { get; set; } = OrderBy.Rating;

        public List<AgeRangeES> Ages { get; set; } = new List<AgeRangeES>() { new AgeRangeES() { MinAge = 0, MaxAge = 100 } };

        public int MinPrice { get; set; } = 0;

        public int MaxPrice { get; set; } = int.MaxValue;

        public List<long> DirectionIds { get; set; } = new List<long> { 0 };

        public string City { get; set; } = "Київ";

        public int Size { get; set; } = 12;

        public int From { get; set; } = 0;
    }
}
