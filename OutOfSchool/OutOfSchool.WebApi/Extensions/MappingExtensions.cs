using System;
using System.Collections.Generic;
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

        public static ApplicationDto ToShortModel(this Application application)
        {
            return Mapper<Application, ApplicationDto>(application, cfg =>
            {
                cfg.CreateMap<Workshop, WorkshopDTO>()
                .ForMember(w => w.Address, m => m.Ignore())
                .ForMember(w => w.Teachers, m => m.Ignore());
                cfg.CreateMap<Child, ChildDto>()
                .ForMember(c => c.BirthCertificate, m => m.Ignore())
                .ForMember(c => c.Parent, m => m.Ignore());
                cfg.CreateMap<Parent, ParentDTO>();
                cfg.CreateMap<Application, ApplicationDto>();
            });
        }

        public static ApplicationDto ToModel(this Application application)
        {
            return Mapper<Application, ApplicationDto>(application, cfg =>
            {
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Teacher, TeacherDTO>();
                cfg.CreateMap<Workshop, WorkshopDTO>()
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title));
                cfg.CreateMap<BirthCertificate, BirthCertificateDto>();
                cfg.CreateMap<Child, ChildDto>()
                .ForMember(c => c.Parent, m => m.Ignore());
                cfg.CreateMap<Parent, ParentDTO>();
                cfg.CreateMap<Application, ApplicationDto>();
            });
        }

        public static BirthCertificateDto ToModel(this BirthCertificate birthCertificate)
        {
            return Mapper<BirthCertificate, BirthCertificateDto>(birthCertificate, cfg => { cfg.CreateMap<BirthCertificate, BirthCertificateDto>(); });
        }

        public static ChildDto ToModel(this Child child)
        {
            return child.Mapper<Child, ChildDto>(cfg =>
            {
                cfg.CreateMap<BirthCertificate, BirthCertificateDto>();
                cfg.CreateMap<Child, ChildDto>();
                cfg.CreateMap<Parent, ParentDTO>();
            });
        }

        public static CityDto ToModel(this City city)
        {
            return Mapper<City, CityDto>(city, cfg => { cfg.CreateMap<City, CityDto>(); });
        }

        public static ClassDto ToModel(this Class classEntity)
        {
            return Mapper<Class, ClassDto>(classEntity, cfg => { cfg.CreateMap<Class, ClassDto>(); });
        }

        public static DepartmentDto ToModel(this Department department)
        {
            return Mapper<Department, DepartmentDto>(department, cfg => { cfg.CreateMap<Department, DepartmentDto>(); });
        }

        public static DirectionDto ToModel(this Direction direction)
        {
            return Mapper<Direction, DirectionDto>(direction, cfg => { cfg.CreateMap<Direction, DirectionDto>(); });
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

        public static TeacherDTO ToModel(this Teacher teacher)
        {
            return Mapper<Teacher, TeacherDTO>(teacher, cfg => { cfg.CreateMap<Teacher, TeacherDTO>(); });
        }

        public static WorkshopDTO ToModel(this Workshop workshop)
        {
            return Mapper<Workshop, WorkshopDTO>(workshop, cfg =>
            {
                cfg.CreateMap<Workshop, WorkshopDTO>()
                    .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Keywords.Split('¤', StringSplitOptions.None)))
                    .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Direction.Title));
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Provider, ProviderDto>();
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
            return Mapper<ApplicationDto, Application>(applicationDTO, cfg =>
            {
                cfg.CreateMap<WorkshopDTO, Workshop>();
                cfg.CreateMap<ChildDto, Child>();
                cfg.CreateMap<ParentDTO, Parent>();
                cfg.CreateMap<ApplicationDto, Application>();
            });
        }

        public static BirthCertificate ToDomain(this BirthCertificateDto birthCertificateDTO)
        {
            return Mapper<BirthCertificateDto, BirthCertificate>(birthCertificateDTO, cfg =>
            {
                cfg.CreateMap<BirthCertificateDto, BirthCertificate>();
            });
        }

        public static Child ToDomain(this ChildDto childDto)
        {
            return Mapper<ChildDto, Child>(childDto, cfg =>
            {
                cfg.CreateMap<BirthCertificateDto, BirthCertificate>();
                cfg.CreateMap<ChildDto, Child>();
                cfg.CreateMap<ParentDTO, Parent>();
            });
        }

        public static City ToDomain(this CityDto cityDto)
        {
            return Mapper<CityDto, City>(cityDto, cfg => { cfg.CreateMap<CityDto, City>(); });
        }

        public static Class ToDomain(this ClassDto classDto)
        {
            return Mapper<ClassDto, Class>(classDto, cfg => { cfg.CreateMap<ClassDto, Class>(); });
        }

        public static Department ToDomain(this DepartmentDto departmentDto)
        {
            return Mapper<DepartmentDto, Department>(departmentDto, cfg => { cfg.CreateMap<DepartmentDto, Department>(); });
        }

        public static Direction ToDomain(this DirectionDto directionDto)
        {
            return Mapper<DirectionDto, Direction>(directionDto, cfg => { cfg.CreateMap<DirectionDto, Direction>(); });
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
                cfg.CreateMap<WorkshopDTO, Workshop>()
                    .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => string.Join('¤', src.Keywords.Distinct())))
                    .ForMember(dest => dest.Direction, opt => opt.Ignore());
                cfg.CreateMap<AddressDto, Address>();
                cfg.CreateMap<ProviderDto, Provider>();
                cfg.CreateMap<TeacherDTO, Teacher>();
            });
        }

        #endregion

        #region ToCard

        public static ParentCardDto ToCard(this ApplicationDto applicationDTO)
        {
            return Mapper<ApplicationDto, ParentCardDto>(applicationDTO, cfg =>
            {
                cfg.CreateMap<ApplicationDto, ParentCardDto>()
                .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ChildId, opt => opt.MapFrom(src => src.ChildId))
                .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(src => src.WorkshopId))
                .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.Workshop.ProviderId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ProviderTitle, opt => opt.MapFrom(src => src.Workshop.ProviderTitle))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Workshop.Rating))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Workshop.Title))
                .ForMember(dest => dest.IsPerMonth, opt => opt.MapFrom(src => src.Workshop.IsPerMonth))
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Workshop.Logo))
                .ForMember(dest => dest.MaxAge, opt => opt.MapFrom(src => src.Workshop.MaxAge))
                .ForMember(dest => dest.MinAge, opt => opt.MapFrom(src => src.Workshop.MinAge))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Workshop.Price))
                .ForMember(dest => dest.Direction, opt => opt.MapFrom(src => src.Workshop.Direction))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Workshop.Address));
            });
        }

        public static WorkshopCard ToCardDto(this WorkshopDTO workshopDTO)
        {
            return Mapper<WorkshopDTO, WorkshopCard>(workshopDTO, cfg =>
            {
                cfg.CreateMap<WorkshopDTO, WorkshopCard>()
                    .ForMember(dest => dest.WorkshopId, opt => opt.MapFrom(s => s.Id))
                    .ForMember(dest => dest.Photo, opt => opt.MapFrom(s => s.Logo));
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