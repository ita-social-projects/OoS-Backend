using Microsoft.Extensions.Hosting;

namespace OutOfSchool.FakeDataSeeder
{
    class Program
    {
        static void Main(string[] args)
        {
            InitialPredefinedData.Create();

            //var host = Host.CreateDefaultBuilder()
            //    .Build();
        }
    }
}
