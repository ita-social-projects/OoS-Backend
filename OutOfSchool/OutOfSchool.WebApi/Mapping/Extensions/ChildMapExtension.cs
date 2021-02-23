using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping.Extensions
{
    public static class ChildMapExtension
    {
        public static ChildDTO ToModel(this Child child)
        {
            var childDto = child.Map<Child, ChildDTO>(
                cfg =>
                {
                    cfg.CreateMap<Child, ChildDTO>();
                });
            return childDto;
        }
        
        public static Child ToDomain(this ChildDTO childDto)
        {
            var child = childDto.Map<ChildDTO, Child>(
                cfg =>
                {
                    cfg.CreateMap<ChildDTO, Child>();
                });
            return child;
        }
    }
}