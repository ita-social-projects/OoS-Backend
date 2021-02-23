using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping.Extensions
{
    public static class WorkshopMapExtension
    {
        public static WorkshopDTO ToModel(this Workshop workshop)
        {
            var workshopDto = workshop.Map<Workshop, WorkshopDTO>(
                cfg =>
                {
                    cfg.CreateMap<Workshop, WorkshopDTO>();
                });
            return workshopDto;
        }
        
        public static Workshop ToDomain(this WorkshopDTO workshopDto)
        {
            var workshop = workshopDto.Map<WorkshopDTO, Workshop>(
                cfg =>
                {
                    cfg.CreateMap<WorkshopDTO, Workshop>();
                });
            return workshop;
        }
    }
}