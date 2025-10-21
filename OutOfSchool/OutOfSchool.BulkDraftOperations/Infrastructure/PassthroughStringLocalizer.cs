using System.Globalization;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.BulkDraftOperations.Infrastructure;

public class PassthroughStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new(name, name, true);

    public LocalizedString this[string name, params object[] arguments] =>
        new(name, string.Format(CultureInfo.CurrentCulture, name, arguments), true);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}