using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping.Extensions
{
    public static class OrganizationMapExtension
    {
        public static OrganizationDTO ToModel(this Organization organization)
        {
            var organizationDto = organization.Map<Organization, OrganizationDTO>(
                cfg =>
                {
                    cfg.CreateMap<Organization, OrganizationDTO>();
                });
            return organizationDto;
        }
        
        public static Organization ToDomain(this OrganizationDTO organizationDto)
        {
            var organization = organizationDto.Map<OrganizationDTO, Organization>(
                cfg =>
                {
                    cfg.CreateMap<OrganizationDTO, Organization>();
                });
            return organization;
        }
    }
}