using System.Collections.Generic;
using OutOfSchool.ElasticsearchData.Enums;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopFilterES
    {
        public string SearchText { get; set; }

        public OrderBy OrderByField { get; set; }

        public List<int[]> Ages { get; set; }

        public int MinPrice { get; set; }

        public int MaxPrice { get; set; }

        public long DirectionId { get; set; }

        public AddressES Address { get; set; }
    }
}
