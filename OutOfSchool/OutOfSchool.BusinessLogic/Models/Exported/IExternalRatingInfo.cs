namespace OutOfSchool.BusinessLogic.Models.Exported;

public interface IExternalRatingInfo : IExternalInfo<Guid>
{
    float Rating { get; set; }

    int NumberOfRatings { get; set; }
}