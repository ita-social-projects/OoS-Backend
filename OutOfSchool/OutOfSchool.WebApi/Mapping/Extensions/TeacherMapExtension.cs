using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Mapping.Extensions
{
    public static class TeacherMapExtension
    {
        public static TeacherDTO ToModel(this Teacher teacher)
        {
            var teacherDto = teacher.Map<Teacher, TeacherDTO>(
                cfg =>
                {
                    cfg.CreateMap<Teacher, TeacherDTO>();
                });
            return teacherDto;
        }
        
        public static Teacher ToDomain(this TeacherDTO teacherDto)
        {
            var teacher = teacherDto.Map<TeacherDTO, Teacher>(
                cfg =>
                {
                    cfg.CreateMap<TeacherDTO, Teacher>();
                });
            return teacher;
        }
    }
}