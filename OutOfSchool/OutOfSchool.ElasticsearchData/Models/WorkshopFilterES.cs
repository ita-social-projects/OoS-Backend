using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.ElasticsearchData.Enums;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopFilterES
    {
        public List<long> Ids { get; set; } = null;

        public string SearchText { get; set; } = string.Empty;

        public OrderBy OrderByField { get; set; } = OrderBy.Rating;

        [ModelBinder(BinderType = typeof(AgeRangeModelBinder))]
        public List<AgeRange> Ages { get; set; } = new List<AgeRange>() { new AgeRange() { MinAge = 0, MaxAge = 100 } };

        public int MinPrice { get; set; } = 0;

        public int MaxPrice { get; set; } = int.MaxValue;

        public List<long> DirectionIds { get; set; } = new List<long> { 0 };

        public string City { get; set; } = "Київ";
    }
}
