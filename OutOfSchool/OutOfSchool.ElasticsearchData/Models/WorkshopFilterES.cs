using System.Collections.Generic;
using OutOfSchool.ElasticsearchData.Enums;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopFilterES
    {
        public List<long> Ids { get; set; } = null;

        public string SearchText { get; set; } = string.Empty;

        public OrderBy OrderByField { get; set; } = OrderBy.Rating;

        public int MinAge { get; set; } = 0;

        public int MaxAge { get; set; } = 100;

        public bool IsFree { get; set; } = false;

        public bool IsPaid { get; set; } = false;

        public int MinPrice { get; set; } = 0;

        public int MaxPrice { get; set; } = int.MaxValue;

        public List<long> DirectionIds { get; set; } = new List<long> { 0 };

        public string City { get; set; } = "Київ";

        public bool WithDisabilityOptions { get; set; } = false;

        public int Size { get; set; } = 12;

        public int From { get; set; } = 0;
    }
}
