using System.Threading.Tasks;

namespace OutOfSchool.RazorTemplatesData.Services;

/// <summary>
/// Defines interface for using Razor engine
/// </summary>
public interface IRazorViewToStringRenderer
{
    /// <summary>
    /// Get rendered string from an html template.
    /// </summary>
    /// <param name="emailName">Email template name.</param>
    /// <param name="model">Data model.</param>
    /// <returns>A <see cref="Task{TResult}"/> Rendered an html string.
    Task<string> GetHtmlStringAsync<TModel>(string emailName, TModel model);

    /// <summary>
    /// Get rendered string from an plain text template.
    /// </summary>
    /// <param name="emailName">Email template name.</param>
    /// <param name="model">Data model.</param>
    /// <returns>A <see cref="Task{TResult}"/> Rendered an plain text string.

    Task<string> GetPlainTextStringAsync<TModel>(string emailName, TModel model);
}