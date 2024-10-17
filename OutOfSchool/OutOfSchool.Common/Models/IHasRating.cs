namespace OutOfSchool.Common.Models;

public interface IHasRating
{
    float Rating { get; }

    int NumberOfRatings { get; }
}