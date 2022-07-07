using System.ComponentModel.DataAnnotations;
﻿using OutOfSchool.Services.Enums;

﻿namespace OutOfSchool.WebApi.Models.Codeficator;

public class CodeficatorFilter
{
    [Required]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets сategories. By default - 'MTCXK'".
    /// </summary>
    public string Categories { get; set; } = CodeficatorCategory.SearchableCategories.Name;
}
