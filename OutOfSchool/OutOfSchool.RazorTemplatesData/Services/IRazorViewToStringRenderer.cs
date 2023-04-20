using System.Threading.Tasks;

namespace OutOfSchool.RazorTemplatesData.Services;

/// <summary>
/// Defines interface for using Razor engine
/// </summary>
public interface IRazorViewToStringRenderer
{
   /// <summary>
   /// Get rendered string from an HTML and plain text template.
   /// </summary>
   /// <param name="emailName"></param>
   /// <param name="model"></param>
   /// <typeparam name="TModel"></typeparam>
   /// <returns>A <see cref="Task{Tuple}"/> rendered an HTML and plain text tuple strings.</returns>
   Task<(string, string)> GetHtmlPlainStringAsync<TModel>(string emailName, TModel model);
}