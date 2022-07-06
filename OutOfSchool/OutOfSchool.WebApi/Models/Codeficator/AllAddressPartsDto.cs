using OutOfSchool.Common;

namespace OutOfSchool.WebApi.Models.Codeficator;

public class AllAddressPartsDto
{
    public string FullAddress
    {
        get
        {
            return GetFullAddress(AddressParts, string.Empty);
        }
    }

    public CodeficatorWithParentDto AddressParts { get; set; }

    private string GetFullAddress(CodeficatorWithParentDto codeficator, string address)
    {
        address += (address.Length == 0 ? string.Empty : Constants.AddressSeparator) + codeficator.Name;

        if (codeficator.Parent != null)
        {
            return GetFullAddress(codeficator.Parent, address);
        }

        return address;
    }
}