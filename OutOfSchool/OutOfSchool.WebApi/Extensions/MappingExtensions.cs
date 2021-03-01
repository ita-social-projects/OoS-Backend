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
            var workshopDto =
                Mapper<Workshop, WorkshopDTO>(workshop, cfg => { cfg.CreateMap<Workshop, WorkshopDTO>(); });
            return workshopDto;
        }

        public static TeacherDTO ToModel(this Teacher teacher)
        {
            var teacherDto = Mapper<Teacher, TeacherDTO>(teacher, cfg => { cfg.CreateMap<Teacher, TeacherDTO>(); });
            return teacherDto;
        }

        public static OrganizationDTO ToModel(this Organization organization)
        {
            var organizationDto = Mapper<Organization, OrganizationDTO>(organization,
                cfg => { cfg.CreateMap<Organization, OrganizationDTO>(); });
            return organizationDto;
        }

        public static ChildDTO ToModel(this Child child)
        {
            var childDto = child.Mapper<Child, ChildDTO>(
                cfg => { cfg.CreateMap<Child, ChildDTO>(); });
            return childDto;
        }

        public static CategoryDTO ToModel(this Category category)
        {
            var categoryDto =
                Mapper<Category, CategoryDTO>(category, cfg => { cfg.CreateMap<Category, CategoryDTO>(); });
            return categoryDto;
        }

        #endregion

        #region ToDomain

        public static Workshop ToDomain(this WorkshopDTO workshopDto)
        {
            var workshop =
                Mapper<WorkshopDTO, Workshop>(workshopDto, cfg => { cfg.CreateMap<WorkshopDTO, Workshop>(); });
            return workshop;
        }
        
        public static Teacher ToDomain(this TeacherDTO teacherDto)
        {
            var teacher = Mapper<TeacherDTO, Teacher>(teacherDto, cfg => { cfg.CreateMap<TeacherDTO, Teacher>(); });
            return teacher;
        }
        
        public static Organization ToDomain(this OrganizationDTO organizationDto)
        {
            var organization = Mapper<OrganizationDTO, Organization>(organizationDto,
                cfg => { cfg.CreateMap<OrganizationDTO, Organization>(); });
            return organization;
        }
        
        public static Child ToDomain(this ChildDTO childDto)
        {
            var child = childDto.Mapper<ChildDTO, Child>(
                cfg => { cfg.CreateMap<ChildDTO, Child>(); });
            return child;
        }

        public static Category ToDomain(this CategoryDTO categoryDto)
        {
            var category =
                Mapper<CategoryDTO, Category>(categoryDto, cfg => { cfg.CreateMap<CategoryDTO, Category>(); });
            return category;
        }

        #endregion
    }
}