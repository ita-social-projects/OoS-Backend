using System.Text.Json.Serialization;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Codeficator;

public class AllAddressPartsDto : CodeficatorAddressDto
{
    public string FullAddress 
        => GetFullAddress(AddressParts, string.Empty, true);

    [JsonIgnore]
    public CodeficatorDto AddressParts { get; set; }

    private string GetFullAddress(CodeficatorDto codeficator, string address, bool isEndPointAddressItem = false)
    {
        if (codeficator == null)
        {
            return address;
        }

        FillAddressPart(codeficator, isEndPointAddressItem);

        address += (address.Length == 0 ? string.Empty : Constants.AddressSeparator) + codeficator.Name;

        if (codeficator.Parent != null)
        {
            return GetFullAddress(codeficator.Parent, address);
        }

        return address;
    }

    private void FillAddressPart(CodeficatorDto codeficator, bool isEndPointAddressItem = false)
    {
        if (isEndPointAddressItem)
        {
            Category = codeficator.Category;
            Latitude = codeficator.Latitude;
            Longitude = codeficator.Longitude;
            Id = codeficator.Id;
        }

        switch (CodeficatorCategory.FromName(codeficator.Category))
        {
            case var e when e.Equals(CodeficatorCategory.Region):
                Region = codeficator.Name;
                break;
            case var e when e.Equals(CodeficatorCategory.District):
                District = codeficator.Name;
                break;
            case var e when e.Equals(CodeficatorCategory.TerritorialCommunity):
                TerritorialCommunity = codeficator.Name;
                break;
            case var e when e.Equals(CodeficatorCategory.City)
                || e.Equals(CodeficatorCategory.UrbanSettlement)
                || e.Equals(CodeficatorCategory.Village)
                || e.Equals(CodeficatorCategory.Settlement):
                Settlement = codeficator.Name;
                break;
            case var e when e.Equals(CodeficatorCategory.CityDistrict):
                CityDistrict = codeficator.Name;
                break;
        }
    }
}

public static class AllAddressPartsDtoExtensions
{
    public static CodeficatorAddressES ToCodeficatorAddressES(this AllAddressPartsDto dto)
        => new()
        {
            // FullAddress - ignored in original AM mapping 
            Id = dto.Id,
            Category = dto.Category,
            ParentId = dto.AddressParts?.ParentId,
            // Parent - ignored in original AM mapping
            Region = dto.Region,
            District = dto.District,
            TerritorialCommunity = dto.TerritorialCommunity,
            Settlement = dto.Settlement,
            CityDistrict = dto.CityDistrict,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Order = dto.Order,
            // FullName - ignored in original AM mapping
        };

    public static AllAddressPartsDto ToAllAddressPartsDto(this CodeficatorAddressES dto)
        => new()
        {
            Id = dto.Id,
            Category = dto.Category,
            Region = dto.Region,
            District = dto.District,
            TerritorialCommunity = dto.TerritorialCommunity,
            Settlement = dto.Settlement,
            CityDistrict = dto.CityDistrict,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Order = dto.Order,
            AddressParts = dto.ToCodeficatorDto()
        };

    public static AllAddressPartsDto ToAllAddressPartsDto(this CATOTTG catottg)
        => new()
        {
            Id = catottg.Id,
            Category = catottg.Category,
            Region = catottg.GetRegionName(),
            District = catottg.GetDistrictName(),
            TerritorialCommunity = catottg.GetTerritorialCommunityName(),
            Settlement = catottg.GetSettlementName(),
            CityDistrict = catottg.GetCityDistrictName(),
            Latitude = catottg.Latitude,
            Longitude = catottg.Longitude,
            Order = catottg.Order,
            AddressParts = catottg.ToCodeficatorDto()
        };
}