namespace OutOfSchool.BusinessLogic.Util.CustomComparers;

public class WorkshopDescriptionItemComparerWithoutKeys : IEqualityComparer<WorkshopDescriptionItem>
{
    public bool Equals(WorkshopDescriptionItem x, WorkshopDescriptionItem y)
    {
        if (x == null && y == null)
            return true;
        if (x == null || y == null)
            return false;

        return x.SectionName.Equals(y.SectionName) && x.Description.Equals(y.Description);
    }

    public int GetHashCode(WorkshopDescriptionItem obj)
    {
        var hash = default(HashCode);
        hash.Add(obj.SectionName);
        hash.Add(obj.Description);

        return hash.ToHashCode();
    }
}