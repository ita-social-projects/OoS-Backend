using System;
using System.Linq;
using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class MappingExtensions
    {
        #region ToModel

        public static AddressDto ToModel(this Address address)
        {
            return Mapper<Address, AddressDto>(address, cfg => { cfg.CreateMap<Address, AddressDto>(); });
        }

        public static ApplicationDto ToModel(this Application application)
        {
            return Mapper<Application, ApplicationDto>(application, cfg => { cfg.CreateMap<Application, ApplicationDto>(); });
        }

        public static BirthCertificateDto ToModel(this BirthCertificate birthCertificate)
        {
            return Mapper<BirthCertificate, BirthCertificateDto>(birthCertificate, cfg => { cfg.CreateMap<BirthCertificate, BirthCertificateDto>(); });
        }

        public static CategoryDTO ToModel(this Category category)
        {
            return Mapper<Category, CategoryDTO>(category, cfg => { cfg.CreateMap<Category, CategoryDTO>(); });
        }

        public static ChildDto ToModel(this Child child)
        {
            return child.Mapper<Child, ChildDto>(cfg =>
            {
                cfg.CreateMap<BirthCertificate, BirthCertificateDto>();
                cfg.CreateMap<Child, ChildDto>();
            });
        }

        public static ParentDTO ToModel(this Parent parent)
        {
            var parentDto =
                Mapper<Parent, ParentDTO>(parent, cfg => { cfg.CreateMap<Parent, ParentDTO>(); });
            return parentDto;
        }

        public static ProviderDto ToModel(this Provider provider)
        {
            return Mapper<Provider, ProviderDto>(provider, cfg =>
            {
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Provider, ProviderDto>()
                 .ForMember(dest => dest.ActualAddress, opt => opt.MapFrom(c => c.ActualAddress))
                 .ForMember(dest => dest.LegalAddress, opt => opt.MapFrom(c => c.LegalAddress));
                cfg.CreateMap<User, UserDto>();
            });
        }

        public static RatingDto ToModel(this Rating rating)
        {
            return Mapper<Rating, RatingDto>(rating, cfg => { cfg.CreateMap<Rating, RatingDto>(); });
        }

        public static ShortUserDto ToModel(this User user)
        {
            return Mapper<User, ShortUserDto>(user, cfg =>
            {
                cfg.CreateMap<User, ShortUserDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(c => c.Id))
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                    .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
                    .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(src => src.IsRegistered))
                    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));
            });
        }

        public static SocialGroupDto ToModel(this SocialGroup group)
        {
            return Mapper<SocialGroup, SocialGroupDto>(group, cfg => { cfg.CreateMap<SocialGroup, SocialGroupDto>(); });
        }

        public static SubcategoryDTO ToModel(this Subcategory category)
        {
            return Mapper<Subcategory, SubcategoryDTO>(category, cfg => { cfg.CreateMap<Subcategory, SubcategoryDTO>(); });
        }

        public static SubsubcategoryDTO ToModel(this Subsubcategory category)
        {
            return Mapper<Subsubcategory, SubsubcategoryDTO>(category, cfg => { cfg.CreateMap<Subsubcategory, SubsubcategoryDTO>(); });
        }

        public static TeacherDTO ToModel(this Teacher teacher)
        {
            return Mapper<Teacher, TeacherDTO>(teacher, cfg => { cfg.CreateMap<Teacher, TeacherDTO>(); });
        }

        public static WorkshopDTO ToModel(this Workshop workshop)
        {
            return Mapper<Workshop, WorkshopDTO>(workshop, cfg =>
            {
                cfg.CreateMap<Workshop, WorkshopDTO>();
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Provider, ProviderDto>();
                cfg.CreateMap<Category, CategoryDTO>();
                cfg.CreateMap<Subcategory, SubcategoryDTO>();
                cfg.CreateMap<Subsubcategory, SubsubcategoryDTO>();
                cfg.CreateMap<Teacher, TeacherDTO>();
            });
        }

        #endregion

        #region ToDomain

        public static Address ToDomain(this AddressDto addressDto)
        {
            return Mapper<AddressDto, Address>(addressDto, cfg => { cfg.CreateMap<AddressDto, Address>(); });
        }

        public static Application ToDomain(this ApplicationDto applicationDTO)
        {
            return Mapper<ApplicationDto, Application>(applicationDTO, cfg => { cfg.CreateMap<ApplicationDto, Application>(); });
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

        public static Child ToDomain(this ChildDto childDto)
        {
            return Mapper<ChildDto, Child>(childDto, cfg =>
            {
                cfg.CreateMap<BirthCertificateDto, BirthCertificate>();
                cfg.CreateMap<ChildDto, Child>();
            });
        }

        public static Parent ToDomain(this ParentDTO parentDto)
        {
            var parent =
                Mapper<ParentDTO, Parent>(parentDto, cfg => { cfg.CreateMap<ParentDTO, Parent>(); });
            return parent;
        }

        public static Provider ToDomain(this ProviderDto providerDto)
        {
            return Mapper<ProviderDto, Provider>(providerDto, cfg =>
            {
                cfg.CreateMap<AddressDto, Address>();
                cfg.CreateMap<ProviderDto, Provider>();
                cfg.CreateMap<UserDto, User>();
            });
        }

        public static Rating ToDomain(this RatingDto ratingDto)
        {
            return Mapper<RatingDto, Rating>(ratingDto, cfg => { cfg.CreateMap<RatingDto, Rating>(); });
        }

        public static SocialGroup ToDomain(this SocialGroupDto groupDto)
        {
            return Mapper<SocialGroupDto, SocialGroup>(groupDto, cfg => { cfg.CreateMap<SocialGroupDto, SocialGroup>(); });
        }

        public static Subcategory ToDomain(this SubcategoryDTO categoryDto)
        {
            return Mapper<SubcategoryDTO, Subcategory>(categoryDto, cfg => { cfg.CreateMap<SubcategoryDTO, Subcategory>(); });
        }

        public static Subsubcategory ToDomain(this SubsubcategoryDTO categoryDto)
        {
            return Mapper<SubsubcategoryDTO, Subsubcategory>(categoryDto, cfg => { cfg.CreateMap<SubsubcategoryDTO, Subsubcategory>(); });
        }

        public static Teacher ToDomain(this TeacherDTO teacherDto)
        {
            return Mapper<TeacherDTO, Teacher>(teacherDto, cfg => { cfg.CreateMap<TeacherDTO, Teacher>(); });
        }

        public static User ToDomain(this ShortUserDto shortUserDto, User user)
        {
            return Mapper<ShortUserDto, User>(shortUserDto, cfg =>
            {
                cfg.CreateMap<ShortUserDto, User>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(c => c.Id))
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                    .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.MiddleName))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                    .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(src => user.IsRegistered))
                    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => user.Role))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => user.Email))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => user.UserName))
                    .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => user.LastLogin))
                    .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => user.NormalizedEmail))
                    .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => user.NormalizedUserName))
                    .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => user.EmailConfirmed))
                    .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => user.PasswordHash))
                    .ForMember(dest => dest.SecurityStamp, opt => opt.MapFrom(src => user.SecurityStamp))
                    .ForMember(dest => dest.ConcurrencyStamp, opt => opt.MapFrom(src => user.ConcurrencyStamp))
                    .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(src => user.PhoneNumberConfirmed))
                    .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(src => user.TwoFactorEnabled))
                    .ForMember(dest => dest.LockoutEnabled, opt => opt.MapFrom(src => user.LockoutEnabled))
                    .ForMember(dest => dest.LockoutEnd, opt => opt.MapFrom(src => user.LockoutEnd))
                    .ForMember(dest => dest.AccessFailedCount, opt => opt.MapFrom(src => user.AccessFailedCount));
            });
        }

        public static Workshop ToDomain(this WorkshopDTO workshopDto)
        {
            return Mapper<WorkshopDTO, Workshop>(workshopDto, cfg =>
            {
                cfg.CreateMap<WorkshopDTO, Workshop>();
                cfg.CreateMap<AddressDto, Address>();
                cfg.CreateMap<ProviderDto, Provider>();
                cfg.CreateMap<CategoryDTO, Category>();
                cfg.CreateMap<SubcategoryDTO, Subcategory>();
                cfg.CreateMap<SubsubcategoryDTO, Subsubcategory>();
                cfg.CreateMap<TeacherDTO, Teacher>();
            });
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
}