﻿namespace OutOfSchool.Services.Models
{
    public class Subcategory
    {
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public Category Category { get; set; }
    }
}