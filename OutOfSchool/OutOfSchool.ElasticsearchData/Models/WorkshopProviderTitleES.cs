﻿namespace OutOfSchool.ElasticsearchData.Models;

public class WorkshopProviderTitleES : IPartial<WorkshopES>
{
    public string ProviderTitle { get; set; }

    public string ProviderTitleEn { get; set; }
}
