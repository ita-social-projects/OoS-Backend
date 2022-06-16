using Ardalis.SmartEnum;

namespace OutOfSchool.Services.Enums
{
    public sealed class CodeficatorCategory : SmartEnum<CodeficatorCategory>
    {
        public static readonly CodeficatorCategory Region = new CodeficatorCategory("O", 1); // Автономна Республіка Крим, області.
        public static readonly CodeficatorCategory SpecialStatusCity = new CodeficatorCategory("K", 2); // Міста, що мають спеціальний статус.
        public static readonly CodeficatorCategory District = new CodeficatorCategory("P", 3); // Райони в областях та Автономній Республіці Крим.
        public static readonly CodeficatorCategory TerritorialCommunity = new CodeficatorCategory("H", 4); // Території територіальних громад (назви територіальних громад) в областях, територіальні громади Автономної Республіки Крим.
        public static readonly CodeficatorCategory City = new CodeficatorCategory("M", 5); // Міста.
        public static readonly CodeficatorCategory UrbanSettlement = new CodeficatorCategory("T", 6); // Селища міського типу.
        public static readonly CodeficatorCategory Village = new CodeficatorCategory("C", 7); // Села.
        public static readonly CodeficatorCategory Settlement = new CodeficatorCategory("X", 8); // Селища.
        public static readonly CodeficatorCategory CityDistrict = new CodeficatorCategory("B", 9); // Райони в містах.

        private CodeficatorCategory(string name, int value)
        : base(name, value)
        {
        }
    }
}
