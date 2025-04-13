namespace OutOfSchool.BusinessLogic.Extensions;
public static class DateOnlyExtensions
{
    /// <summary>
    /// Converts any DateOnly to a StudyPeriod date by preserving Month and Day, setting Year to 2000.
    /// </summary>
    /// <param name="date">The date to convert.</param>
    /// <returns>A new DateOnly instance with the year set to 2000 and the month and day preserved.</returns>
    public static DateOnly ToStudyPeriodDate(this DateOnly date) 
        => new DateOnly(2000, date.Month, date.Day);
}