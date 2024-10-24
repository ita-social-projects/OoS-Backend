namespace OutOfSchool.BusinessLogic.Services;

public interface IValueProjector
{
    string ProjectValue(Type type, object value);
}