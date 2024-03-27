namespace OutOfSchool.WebApi.Services;

public interface IValueProjector
{
    string ProjectValue(Type type, object value);
}