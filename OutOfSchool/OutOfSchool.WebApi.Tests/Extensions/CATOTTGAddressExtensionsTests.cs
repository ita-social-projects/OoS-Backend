using NUnit.Framework;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class CATOTTGAddressExtensionsTests
{
    private static CATOTTG cityDistrictWithParent;
    private static CATOTTG settlementWithParent;
    private static CATOTTG territorialCommunityWithParent;
    private static CATOTTG districtWithParent;
    private static CATOTTG regionWithoutParent;
    private static CATOTTG cityDistrictWithoutParent;
    private static CATOTTG settlementWithoutParent;

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        regionWithoutParent = new CATOTTG()
        {
            Id = 5,
            ParentId = null,
            Parent = null,
            Name = "Чернігівська",
            Category = "O",
        };

        districtWithParent = new CATOTTG()
        {
            Id = 4,
            ParentId = 5,
            Parent = regionWithoutParent,
            Name = "Корюківський",
            Category = "P",
        };

        territorialCommunityWithParent = new CATOTTG()
        {
            Id = 3,
            ParentId = 4,
            Parent = districtWithParent,
            Name = "Корюківська",
            Category = "H",
        };

        settlementWithParent = new CATOTTG()
        {
            Id = 2,
            ParentId = 3,
            Parent = territorialCommunityWithParent,
            Name = "Корюківка",
            Category = "M",
        };

        cityDistrictWithParent = new CATOTTG()
        {
            Id = 1,
            ParentId = 2,
            Parent = settlementWithParent,
            Name = "Залізничний",
            Category = "B",
        };

        cityDistrictWithoutParent = new CATOTTG()
        {
            Id = 1,
            ParentId = null,
            Parent = null,
            Name = "Печерський",
            Category = "B",
        };

        settlementWithoutParent = new CATOTTG()
        {
            Id = 7,
            ParentId = null,
            Parent = null,
            Name = "Київ",
            Category = "K",
        };
    }

    #region GetCityDistrictName
    [Test]
    public void GetCityDistrictName_WhenCATOTTGIsCityDistrict_ShouldReturnCityDistrictName()
    {
        // Arrange
        var expected = "Залізничний";

        // Act
        var result = CatottgAddressExtensions.GetCityDistrictName(cityDistrictWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetCityDistrictName_WhenCATOTTGIsNotCityDistrict_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetCityDistrictName(settlementWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion

    #region GetSettlementName
    [Test]
    public void GetSettlementName_WhenCATOTTGIsSettlement_ShouldReturnSettlementName()
    {
        // Arrange
        string expected = "Корюківка";

        // Act
        var result = CatottgAddressExtensions.GetSettlementName(settlementWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetSettlementName_WhenCATOTTGIsCityDistrictWithParent_ShouldReturnParentName()
    {
        // Arrange
        string expected = "Корюківка";

        // Act
        var result = CatottgAddressExtensions.GetSettlementName(cityDistrictWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion

    #region GetTerritorialCommunityName
    [Test]
    public void GetTerritorialCommunityName_WhenCATOTTGIsCityDistrictWithParent_ShouldReturnTerritorialCommunityName()
    {
        // Arrange
        string expected = "Корюківська";

        // Act
        var result = CatottgAddressExtensions.GetTerritorialCommunityName(cityDistrictWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTerritorialCommunityName_WhenCATOTTGIsSettlementWithParent_ShouldReturnTerritorialCommunityName()
    {
        // Arrange
        string expected = "Корюківська";

        // Act
        var result = CatottgAddressExtensions.GetTerritorialCommunityName(settlementWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTerritorialCommunityName_WhenCATOTTGIsSettlementWithoutParent_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetTerritorialCommunityName(settlementWithoutParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetTerritorialCommunityName_WhenCATOTTGIsCityDistrictWithoutParent_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetTerritorialCommunityName(cityDistrictWithoutParent);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion

    #region GetDistrictName
    [Test]
    public void GetDistrictName_WhenCATOTTGIsCityDistrictWithParent_ShouldReturnDistrictName()
    {
        // Arrange
        string expected = "Корюківський";

        // Act
        var result = CatottgAddressExtensions.GetDistrictName(cityDistrictWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetDistrictName_WhenCATOTTGIsSettlementWithParent_ShouldReturnDistrictName()
    {
        // Arrange
        string expected = "Корюківський";

        // Act
        var result = CatottgAddressExtensions.GetDistrictName(settlementWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetDistrictName_WhenCATOTTGIsSettlementWithoutParent_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetDistrictName(settlementWithoutParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetDistrictName_WhenCATOTTGIsCityDistrictWithoutParent_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetDistrictName(cityDistrictWithoutParent);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion

    #region GetRegionName
    [Test]
    public void GetRegionName_WhenCATOTTGIsCityDistrictWithParent_ShouldReturnRegionName()
    {
        // Arrange
        string expected = "Чернігівська";

        // Act
        var result = CatottgAddressExtensions.GetRegionName(cityDistrictWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetRegionName_WhenCATOTTGIsSettlementWithParent_ShouldReturnRegionName()
    {
        // Arrange
        string expected = "Чернігівська";

        // Act
        var result = CatottgAddressExtensions.GetRegionName(settlementWithParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetRegionName_WhenCATOTTGIsSettlementWithoutParent_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetRegionName(settlementWithoutParent);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void GetRegionName_WhenCATOTTGIsCityDistrictWithoutParent_ShouldReturnNull()
    {
        // Arrange
        string expected = null;

        // Act
        var result = CatottgAddressExtensions.GetRegionName(cityDistrictWithoutParent);

        // Assert
        Assert.AreEqual(expected, result);
    }
    #endregion
}
