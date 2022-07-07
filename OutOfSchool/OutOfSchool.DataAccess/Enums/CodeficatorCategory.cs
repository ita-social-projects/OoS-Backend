using Ardalis.SmartEnum;

namespace OutOfSchool.Services.Enums;

public sealed class CodeficatorCategory : SmartEnum<CodeficatorCategory>
{
    public static readonly CodeficatorCategory Region = new ("O", 1); // Автономна Республіка Крим, області.
    public static readonly CodeficatorCategory SpecialStatusCity = new ("K", 2); // Міста, що мають спеціальний статус.
    public static readonly CodeficatorCategory Level1 = new ("OK", 3); // Автономна Республіка Крим, області та міста, що мають спеціальний статус.

    public static readonly CodeficatorCategory District = new ("P", 4); // Райони в областях та Автономній Республіці Крим.

    public static readonly CodeficatorCategory TerritorialCommunity = new ("H", 8); // Території територіальних громад (назви територіальних громад) в областях, територіальні громади Автономної Республіки Крим.
    public static readonly CodeficatorCategory Level3 = new ("H", 8); // Території територіальних громад (назви територіальних громад) в областях, територіальні громади Автономної Республіки Крим.

    public static readonly CodeficatorCategory City = new ("M", 16); // Міста.
    public static readonly CodeficatorCategory UrbanSettlement = new ("T", 32); // Селища міського типу.
    public static readonly CodeficatorCategory Village = new ("C", 64); // Села.
    public static readonly CodeficatorCategory Settlement = new ("X", 128); // Селища.
    public static readonly CodeficatorCategory Level4 = new ("MTCX", 208); // Міста, селища міського типу, села та селища

    public static readonly CodeficatorCategory CityDistrict = new ("B", 256); // Райони в містах.
    public static readonly CodeficatorCategory Level2 = new ("PB", 260); // Райони в областях та Автономній Республіці Крим та райони в містах.

    public static readonly CodeficatorCategory SearchableCategories = new ("MTCXK", 210); // Міста, що мають спеціальний статус, міста, селища міського типу, села та селища

    private CodeficatorCategory(string name, int value)
    : base(name, value)
    {
    }
}