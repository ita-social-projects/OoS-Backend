namespace OutOfSchool.BusinessLogic.Models.Position;
public class PositionDto // for get method
{
    public Guid Id { get; set; }

    public string Language { get; set; }

    public string Description { get; set; }

    public bool IsForRuralAreas { get; set; }

    public string Department { get; set; }

    public int SeatsAmount { get; set; }

    public string FullName { get; set; }

    public string ShortName { get; set; }

    public string GenitiveName { get; set; }

    public bool IsTeachingPosition { get; set; }

    public float Rate { get; set; }

    public float Tariff { get; set; }

    public string ClassifierType { get; set; }

    public Guid ProviderId { get; set; }

    public Guid ContactId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool IsDeleted { get; set; }
}
