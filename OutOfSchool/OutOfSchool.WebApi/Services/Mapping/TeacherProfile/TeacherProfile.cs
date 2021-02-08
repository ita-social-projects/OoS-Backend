using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;

namespace OutOfSchool.WebApi.Services.Mapping.TeacherProfile
{
    public class TeacherProfile : Profile
    {
        public TeacherProfile()
        {
            CreateMap<TeacherDto, Teacher>();
        }
    }
}