namespace OutOfSchool.WebApi.Models
{
    public class ParentDtoWithShortUserInfo : ParentDTO
    {
        public ShortUserDto User { get; set; }
    }
}
