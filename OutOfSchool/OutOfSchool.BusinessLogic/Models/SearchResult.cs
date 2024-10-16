﻿namespace OutOfSchool.BusinessLogic.Models;

public class SearchResult<TEntity>
{
    public int TotalAmount { get; set; }

    public IReadOnlyCollection<TEntity> Entities { get; set; }
}