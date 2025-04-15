using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.Common.Models;

public interface IHasHiddenFields
{
    bool IsSelfFinanced { get; set; }
    bool IsInclusive { get; set; }
    SpecialNeedsType SpecialNeedsType { get; set; }
    EducationalShift EducationalShift { get; set; }
    AgeComposition AgeComposition { get; set; }
}
