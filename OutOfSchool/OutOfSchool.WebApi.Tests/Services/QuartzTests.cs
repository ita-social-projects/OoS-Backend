using System.IO;
using System.Linq;
using NUnit.Framework;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class QuartzTests
    {
        // If this migration file was deleted we have to restore it
        // from db-quartzmigration.txt file in IdentityServer project.
        [Test]
        public void CheckExistingMigrationFile_ReturnsTrueIfExists()
        {
            // Arrange
            DirectoryInfo directory = TryGetSolutionDirectoryInfo();
            string path = @"OutOfSchool.IdentityServer\Data\Migrations\OutOfSchoolMigrations\20220523184345_Quartz.cs";
            path = Path.Combine(directory.FullName, path);

            // Act
            bool exists = File.Exists(path);

            // Assert
            Assert.IsTrue(exists);
        }

        private DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(currentPath ?? Directory.GetCurrentDirectory());

            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory;
        }
    }
}
