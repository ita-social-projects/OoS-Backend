using Ardalis.SmartEnum;

namespace OutOfSchool.Services.Enums;

public abstract class CodeficatorCategory : SmartEnum<CodeficatorCategory>
{
    public static readonly CodeficatorCategory Region = new RegionCategory(); // Автономна Республіка Крим, області.

    public static readonly CodeficatorCategory SpecialStatusCity = new SpecialStatusCityCategory(); // Міста, що мають спеціальний статус.

    public static readonly CodeficatorCategory District = new DistrictCategory(); // Райони в областях та Автономній Республіці Крим.

    public static readonly CodeficatorCategory TerritorialCommunity = new TerritorialCommunityCategory(); // Території територіальних громад (назви територіальних громад) в областях, територіальні громади Автономної Республіки Крим.

    public static readonly CodeficatorCategory City = new CityCategory(); // Міста.

    public static readonly CodeficatorCategory UrbanSettlement = new UrbanSettlementCategory(); // Селища міського типу.

    public static readonly CodeficatorCategory Village = new VillageCategory(); // Села.

    public static readonly CodeficatorCategory Settlement = new SettlementCategory(); // Селища.

    public static readonly CodeficatorCategory CityDistrict = new CityDistrictCategory(); // Райони в містах.

    public static readonly CodeficatorCategory Level1 = new Level1Category(); // Автономна Республіка Крим, області та міста, що мають спеціальний статус.

    public static readonly CodeficatorCategory Level2 = new Level2Category(); // Райони в областях та Автономній Республіці Крим та райони в містах.

    public static readonly CodeficatorCategory Level4 = new Level4Category(); // Міста, селища міського типу, села та селища

    public static readonly CodeficatorCategory SearchableCategories = new SearchableCategoriesCategory(); // Міста, що мають спеціальний статус, міста, селища міського типу, села та селища

    private CodeficatorCategory(string name, int value)
    : base(name, value)
    {
    }

    public abstract string Abbrivation { get; }

    private sealed class RegionCategory : CodeficatorCategory
    {
        public RegionCategory()
            : base("O", 1)
        {
        }

        public override string Abbrivation => "обл.";
    }

    private sealed class SpecialStatusCityCategory : CodeficatorCategory
    {
        public SpecialStatusCityCategory()
            : base("K", 2)
        {
        }

        public override string Abbrivation => "м.";
    }

    private sealed class DistrictCategory : CodeficatorCategory
    {
        public DistrictCategory()
            : base("P", 4)
        {
        }

        public override string Abbrivation => "р-н";
    }

    private sealed class TerritorialCommunityCategory : CodeficatorCategory
    {
        public TerritorialCommunityCategory()
            : base("H", 8)
        {
        }

        public override string Abbrivation => "отг";
    }

    private sealed class CityCategory : CodeficatorCategory
    {
        public CityCategory()
            : base("M", 16)
        {
        }

        public override string Abbrivation => "м.";
    }

    private sealed class UrbanSettlementCategory : CodeficatorCategory
    {
        public UrbanSettlementCategory()
            : base("T", 32)
        {
        }

        public override string Abbrivation => "смт";
    }

    private sealed class VillageCategory : CodeficatorCategory
    {
        public VillageCategory()
            : base("C", 64)
        {
        }

        public override string Abbrivation => "с.";
    }

    private sealed class SettlementCategory : CodeficatorCategory
    {
        public SettlementCategory()
            : base("X", 128)
        {
        }

        public override string Abbrivation => "с-ще";
    }

    private sealed class CityDistrictCategory : CodeficatorCategory
    {
        public CityDistrictCategory()
            : base("B", 258)
        {
        }

        public override string Abbrivation => "р-н";
    }

    // Group (flag) values

    private sealed class Level1Category : CodeficatorCategory
    {
        public Level1Category()
            : base("OK", 3)
        {
        }

        public override string Abbrivation => string.Empty;
    }

    private sealed class Level2Category : CodeficatorCategory
    {
        public Level2Category()
            : base("PB", 260)
        {
        }

        public override string Abbrivation => string.Empty;
    }

    private sealed class Level4Category : CodeficatorCategory
    {
        public Level4Category()
            : base("MTCX", 208)
        {
        }

        public override string Abbrivation => string.Empty;
    }

    private sealed class SearchableCategoriesCategory : CodeficatorCategory
    {
        public SearchableCategoriesCategory()
            : base("MTCXK", 210)
        {
        }

        public override string Abbrivation => string.Empty;
    }
}