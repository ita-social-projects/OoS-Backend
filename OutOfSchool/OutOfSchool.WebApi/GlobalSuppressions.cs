// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1012:OpeningBracesMustBeSpacedCorrectly", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1013:ClosingBracesMustBeSpacedCorrectly", Justification = "Reviewed.")]
[assembly: SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1200:Using directives should be placed correctly", Justification = "Correct placement of the using directive does not affect project performance.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "The names of the enum elements should be informative enough on their own.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Model is a simple self-documenting class.")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "We don't have to apply keyword this to every element.")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:File should have header", Justification = "We don't have to use headers in the project")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1028:Code must not contain trailing whitespace", Justification = "New rule that requires too much work to implement")]