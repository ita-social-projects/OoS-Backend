namespace OutOfSchool.WebApi.Models;

/// <summary>
/// The filter to take specified amount of entites from the collection.
/// </summary>
public class SizeFilter
{
    private const int MaxSize = 100;

    private int size = 10;

    /// <summary>
    /// Gets or sets the amount of entities to take from collection.
    /// </summary>
    public int Size
    {
        get => size;

        set => size = (value > MaxSize) ? MaxSize : value;
    }
}