using OutOfSchool.BusinessLogic.Models.Codeficator;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.AddressDraft;

public class AddressDraftResponseDto: AddressDraftBaseDto
{
    public AllAddressPartsDto CodeficatorAddressDto { get; set; }
}