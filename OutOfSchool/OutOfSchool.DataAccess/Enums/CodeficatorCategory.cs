using Ardalis.SmartEnum;

namespace OutOfSchool.Services.Enums
{
    public sealed class CodeficatorCategory : SmartEnum<CodeficatorCategory>
    {
        public static readonly CodeficatorCategory Region = new CodeficatorCategory("O", 1); // Автономна Республіка Крим, області.
        public static readonly CodeficatorCategory SpecialStatusCity = new CodeficatorCategory("K", 2); // Міста, що мають спеціальний статус.
        public static readonly CodeficatorCategory Level1 = new CodeficatorCategory("OK", 3); // Автономна Республіка Крим, області та міста, що мають спеціальний статус.

        public static readonly CodeficatorCategory District = new CodeficatorCategory("P", 4); // Райони в областях та Автономній Республіці Крим.

        public static readonly CodeficatorCategory TerritorialCommunity = new CodeficatorCategory("H", 8); // Території територіальних громад (назви територіальних громад) в областях, територіальні громади Автономної Республіки Крим.
        public static readonly CodeficatorCategory Level3 = new CodeficatorCategory("H", 8); // Території територіальних громад (назви територіальних громад) в областях, територіальні громади Автономної Республіки Крим.

        public static readonly CodeficatorCategory City = new CodeficatorCategory("M", 16); // Міста.
        public static readonly CodeficatorCategory UrbanSettlement = new CodeficatorCategory("T", 32); // Селища міського типу.
        public static readonly CodeficatorCategory Village = new CodeficatorCategory("C", 64); // Села.
        public static readonly CodeficatorCategory Settlement = new CodeficatorCategory("X", 128); // Селища.
        public static readonly CodeficatorCategory Level4 = new CodeficatorCategory("MTCX", 208); // Міста, селища міського типу, села та селища

        public static readonly CodeficatorCategory CityDistrict = new CodeficatorCategory("B", 256); // Райони в містах.
        public static readonly CodeficatorCategory Level2 = new CodeficatorCategory("PB", 260); // Райони в областях та Автономній Республіці Крим та райони в містах.

        private CodeficatorCategory(string name, int value)
        : base(name, value)
        {
        }
    }
}
