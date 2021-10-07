﻿using System.Collections.Generic;
using OutOfSchool.ElasticsearchData.Enums;

namespace OutOfSchool.ElasticsearchData.Models
{
    public class WorkshopFilterES
    {
        public List<long> Ids { get; set; } = new List<long>();

        public string SearchText { get; set; } = string.Empty;

        public string OrderByField { get; set; } = OrderBy.Id.ToString();

        public int MinAge { get; set; } = 0;

        public int MaxAge { get; set; } = 100;

        public bool IsFree { get; set; } = false;

        public int MinPrice { get; set; } = 0;

        public int MaxPrice { get; set; } = int.MaxValue;

        public List<long> DirectionIds { get; set; } = new List<long>();

        public string City { get; set; } = string.Empty;

        public bool WithDisabilityOptions { get; set; } = false;

        public string Workdays { get; set; } = string.Empty;

        public int StartHour { get; set; } = 0;

        public int EndHour { get; set; } = 23;

        public int Size { get; set; } = 12;

        public int From { get; set; } = 0;
    }
}
