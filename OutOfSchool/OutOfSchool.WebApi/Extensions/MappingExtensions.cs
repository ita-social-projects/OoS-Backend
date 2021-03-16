using System;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class MappingExtensions
    {
        private static TDestination Mapper<TSource, TDestination>(
            this TSource source,
            Action<IMapperConfigurationExpression> configure)
        {
            var config = new MapperConfiguration(configure);
            var mapper = config.CreateMapper();
            var destination = mapper.Map<TDestination>(source);
            return destination;
        }

        #region ToModel

        public static WorkshopDTO ToModel(this Workshop workshop)
        {
            return Mapper<Workshop, WorkshopDTO>(workshop, cfg => { cfg.CreateMap<Workshop, WorkshopDTO>(); });
        }

        public static TeacherDTO ToModel(this Teacher teacher)
        {
            return Mapper<Teacher, TeacherDTO>(teacher, cfg => { cfg.CreateMap<Teacher, TeacherDTO>(); });;
        }

        public static OrganizationDTO ToModel(this Organization organization)
        {
            return Mapper<Organization, OrganizationDTO>(organization, cfg => { cfg.CreateMap<Organization, OrganizationDTO>(); });;
        }

        public static ChildDTO ToModel(this Child child)
        {
            return child.Mapper<Child, ChildDTO>(cfg => { cfg.CreateMap<Child, ChildDTO>(); });
        }

        public static CategoryDTO ToModel(this Category category)
        {
            return Mapper<Category, CategoryDTO>(category, cfg => { cfg.CreateMap<Category, CategoryDTO>(); });
        }

        #endregion

        #region ToDomain

        public static Workshop ToDomain(this WorkshopDTO workshopDto)
        {
            return Mapper<WorkshopDTO, Workshop>(workshopDto, cfg =>
            {
                cfg.CreateMap<WorkshopDTO, Workshop>()
                    .ForMember(dest => dest.Address, opt => opt.Ignore())
                    .ForMember(dest => dest.Organization, opt => opt.Ignore())
                    .ForMember(dest => dest.Teachers, opt => opt.Ignore())
                    .ForMember(dest => dest.Category, opt => opt.Ignore());
            });
        }

        public static Teacher ToDomain(this TeacherDTO teacherDto)
        {
            return Mapper<TeacherDTO, Teacher>(teacherDto, cfg => { cfg.CreateMap<TeacherDTO, Teacher>(); });
        }

        public static Organization ToDomain(this OrganizationDTO organizationDto)
        {
            return Mapper<OrganizationDTO, Organization>(organizationDto,
                cfg => { cfg.CreateMap<OrganizationDTO, Organization>(); });
        }

        public static Child ToDomain(this ChildDTO childDto)
        {
            return childDto.Mapper<ChildDTO, Child>(cfg => { cfg.CreateMap<ChildDTO, Child>(); });
        }

        public static Category ToDomain(this CategoryDTO categoryDto)
        {
            return Mapper<CategoryDTO, Category>(categoryDto, cfg => { cfg.CreateMap<CategoryDTO, Category>(); });
        }

        #endregion
    }
}