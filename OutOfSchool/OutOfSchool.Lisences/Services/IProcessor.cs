namespace OutOfSchool.Licenses.Services;

public interface IProcessor<out T>
{
    T Process();
}