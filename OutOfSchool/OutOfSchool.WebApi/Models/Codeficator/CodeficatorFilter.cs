using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Codeficator;

public class CodeficatorFilter
{
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets сategories. By default - 'MTCXK'".
    /// </summary>
    public string Categories { get; set; } = CodeficatorCategory.SearchableCategories.Name;
}