using System;
using System.Linq;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class MappingExtensions
    {
#pragma warning disable SA1124 // Do not use regions

        #region ToModel

        public static AddressDto ToModel(this Address address)
        {          
            return Mapper<Address, AddressDto>(address, cfg => { cfg.CreateMap<Address, AddressDto>(); });
        }

        public static WorkshopDTO ToModel(this Workshop workshop)
        {
            return Mapper<Workshop, WorkshopDTO>(workshop, cfg => { cfg.CreateMap<Workshop, WorkshopDTO>(); });
        }

        public static TeacherDTO ToModel(this Teacher teacher)
        {
            return Mapper<Teacher, TeacherDTO>(teacher, cfg => { cfg.CreateMap<Teacher, TeacherDTO>(); });
        }

        public static ProviderDto ToModel(this Provider provider)
        {
            return Mapper<Provider, ProviderDto>(provider, cfg =>
            {
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Provider, ProviderDto>()
                 .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(c => c.ActualAddress))               
                 .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(c => c.LegalAddress));              
            });
        }

        public static ChildDTO ToModel(this Child child)
        {
            return child.Mapper<Child, ChildDTO>(cfg => { cfg.CreateMap<Child, ChildDTO>(); });
        }

        public static BirthCertificateDto ToModel(this BirthCertificate birthCertificate)
        {
            return Mapper<BirthCertificate, BirthCertificateDto>(birthCertificate, cfg => { cfg.CreateMap<BirthCertificate, BirthCertificateDto>(); });
        }

        public static CategoryDTO ToModel(this Category category)
        {
            return Mapper<Category, CategoryDTO>(category, cfg => { cfg.CreateMap<Category, CategoryDTO>(); });
        }

        public static ParentDTO ToModel(this Parent parent)
        {
            var parentDto =
                Mapper<Parent, ParentDTO>(parent, cfg => { cfg.CreateMap<Parent, ParentDTO>(); });
            return parentDto;
        }

        #endregion

        #region ToDomain

        public static Address ToDomain(this AddressDto addressDto)
        {                   
            return Mapper<AddressDto, Address>(addressDto, cfg => { cfg.CreateMap<AddressDto, Address>(); });
        }

        public static Workshop ToDomain(this WorkshopDTO workshopDto)
        {
            return Mapper<WorkshopDTO, Workshop>(workshopDto, cfg =>
            {
                cfg.CreateMap<ProviderDto, Provider>();

                cfg.CreateMap<WorkshopDTO, Workshop>()
                    .ForMember(dest => dest.Address, opt => opt.Ignore())
                    .ForMember(dest => dest.Teachers, opt => opt.Ignore())
                    .ForMember(dest => dest.Category, opt => opt.Ignore());
            });
        }

        public static Teacher ToDomain(this TeacherDTO teacherDto)
        {
            return Mapper<TeacherDTO, Teacher>(teacherDto, cfg => { cfg.CreateMap<TeacherDTO, Teacher>(); });
        }

        public static Provider ToDomain(this ProviderDto providerDto)
        {
            return Mapper<ProviderDto, Provider>(providerDto, cfg =>
            {
                cfg.CreateMap<AddressDto, Address>();
                cfg.CreateMap<ProviderDto, Provider>();               
            });
        }

        public static Child ToDomain(this ChildDTO childDto)
        {
            return Mapper<ChildDTO, Child>(childDto, cfg =>
            {
                cfg.CreateMap<AddressDto, Address>();
                cfg.CreateMap<BirthCertificateDto, BirthCertificate>();

                cfg.CreateMap<ChildDTO, Child>()
                    .ForMember(dest => dest.AddressId, opt => opt.MapFrom(c => c.Address.Id));
            });
        }

        public static BirthCertificate ToDomain(this BirthCertificateDto birthCertificateDTO)
        {
            return Mapper<BirthCertificateDto, BirthCertificate>(birthCertificateDTO, cfg =>
            {
                cfg.CreateMap<BirthCertificateDto, BirthCertificate>();
            });
        }

        public static Category ToDomain(this CategoryDTO categoryDto)
        {
            return Mapper<CategoryDTO, Category>(categoryDto, cfg => { cfg.CreateMap<CategoryDTO, Category>(); });
        }

        public static Parent ToDomain(this ParentDTO parentDto)
        {
            var parent =
                Mapper<ParentDTO, Parent>(parentDto, cfg => { cfg.CreateMap<ParentDTO, Parent>(); });
            return parent;
        }

        #endregion

        private static TDestination Mapper<TSource, TDestination>(
            this TSource source,
            Action<IMapperConfigurationExpression> configure)
        {
            var config = new MapperConfiguration(configure);
            var mapper = config.CreateMapper();
            var destination = mapper.Map<TDestination>(source);
            return destination;
        }
    }
#pragma warning restore SA1124 // Do not use regions
}