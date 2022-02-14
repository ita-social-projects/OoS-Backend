using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Config
{
    public class ApplicationsConstraintsConfig
    {
        public const string Name = "ApplicationsConstraints";

        [Range(1, int.MaxValue, ErrorMessage = "ApplicationsLimit must be greater then 0.")]
        public int ApplicationsLimit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ApplicationsLimitDays must be greater then 0.")]
        public int ApplicationsLimitDays { get; set; }
    }
}
