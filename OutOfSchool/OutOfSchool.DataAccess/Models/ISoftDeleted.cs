namespace OutOfSchool.Services.Models;

public interface ISoftDeleted
{
    bool IsDeleted { get; set; }
}
