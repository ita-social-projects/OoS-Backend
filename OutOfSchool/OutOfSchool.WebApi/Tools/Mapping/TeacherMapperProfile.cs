using AutoMapper;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Tools.Mapping
{
    /// <summary>
    /// Mapper of Teacher to TeacherDto.
    /// </summary>
    public class TeacherMapperProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherMapperProfile"/> class.
        /// </summary>
        public TeacherMapperProfile()
        {
            CreateMap<TeacherDTO, Teacher>();
        }
    }
}