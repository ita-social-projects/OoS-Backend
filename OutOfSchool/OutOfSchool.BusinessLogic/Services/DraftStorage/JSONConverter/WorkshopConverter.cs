using Newtonsoft.Json.Linq;
using OutOfSchool.BusinessLogic.Models.Workshops.IncompletedWorkshops;

namespace OutOfSchool.BusinessLogic.Services.DraftStorage.JSONConverter;

public class WorkshopConverter : JsonCreationConverter<WorkshopWithRequiredPropertiesDto>
{
    protected override WorkshopWithRequiredPropertiesDto Create(Type objectType, JObject jObject)
    {
        ArgumentNullException.ThrowIfNull(objectType);
        ArgumentNullException.ThrowIfNull(jObject);

        if (jObject["teachers"] != null)
        {
            return new WorkshopWithTeachersDto();
        }
        else if (jObject["addressId"] != null && jObject["address"] != null)
        {
            return new WorkshopWithContactsDto();
        }
        else if (jObject["workshopDescriptionItems"] != null)
        {
            return new WorkshopWithDescriptionDto();
        }
        else
        {
            return new WorkshopWithRequiredPropertiesDto();
        }
    }
}
