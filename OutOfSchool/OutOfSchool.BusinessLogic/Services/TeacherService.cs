using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Teacher entity.
/// </summary>
/// <param name="teacherRepository">Repository for Teacher entity.</param>
/// <param name="teacherImagesService">Teacher images mediator.</param>
/// <param name="logger">Logger.</param>
public class TeacherService(
    ISensitiveEntityRepositorySoftDeleted<Teacher> teacherRepository, 
    IEntityCoverImageInteractionService<Teacher> teacherImagesService, 
    ILogger<TeacherService> logger
) : ITeacherService
{
    /// <inheritdoc/>
    public async Task<TeacherCreationResultDto> Create(TeacherDTO dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation("Teacher creating was started.");

        var newTeacher = await teacherRepository
            .Create(dto.ToModel(default, dto.WorkshopId))
            .ConfigureAwait(false);

        Result<string> uploadingResult = null;
        if (dto.CoverImage != null)
        {
            uploadingResult = await teacherImagesService.AddCoverImageAsync(newTeacher, dto.CoverImage).ConfigureAwait(false);
            if (uploadingResult.Succeeded)
            {
                dto.ToModel(default, dto.WorkshopId).CoverImageId = uploadingResult.Value;
                await UpdateTeacher().ConfigureAwait(false);
            }
        }

        logger.LogInformation($"Teacher with Id = {newTeacher.Id} created successfully.");

        return new TeacherCreationResultDto
        {
            Teacher = newTeacher.ToDto(),
            UploadingAvatarImageResult = uploadingResult?.OperationResult,
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TeacherDTO>> GetAll()
    {
        logger.LogInformation("Getting all Teachers started.");

        var teachers = await teacherRepository.GetAll().ConfigureAwait(false);

        logger.LogInformation(!teachers.Any()
            ? "Teacher table is empty."
            : $"All {teachers.Count()} records were successfully received from the Teacher table");

        return teachers.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TeacherDTO> GetById(Guid id)
    {
        logger.LogInformation($"Getting Teacher by Id started. Looking Id = {id}.");

        var teacher = await teacherRepository.GetById(id).ConfigureAwait(false);

        if (teacher == null)
        {
            throw new ArgumentException(
                nameof(id),
                paramName: $"There are no recors in teachers table with such id - {id}.");
        }

        logger.LogInformation($"Got a Teacher with Id = {id}.");

        return teacher.ToDto();
    }

    /// <inheritdoc/>
    public async Task<TeacherUpdateResultDto> Update(TeacherDTO dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogInformation($"Updating Teacher with Id = {dto.Id} started.");

        var teacher = await teacherRepository.GetById(dto.Id).ConfigureAwait(false);

        teacher = dto.SetToModel(teacher);

        var changingAvatarResult = await teacherImagesService.ChangeCoverImageAsync(teacher, dto.CoverImageId, dto.CoverImage).ConfigureAwait(false);

        await UpdateTeacher().ConfigureAwait(false);

        return new TeacherUpdateResultDto
        {
            Teacher = teacher.ToDto(),
            UploadingAvatarImageResult = changingAvatarResult?.UploadingResult?.OperationResult,
        };
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogInformation($"Deleting Teacher with Id = {id} started.");

        var entity = await teacherRepository.GetById(id).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(entity.CoverImageId))
        {
            await teacherImagesService.RemoveCoverImageAsync(entity).ConfigureAwait(false);
        }

        try
        {
            await teacherRepository.Delete(entity).ConfigureAwait(false);

            logger.LogInformation($"Teacher with Id = {id} successfully deleted.");
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Deleting Teacher with Id = {id} failed.");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Guid> GetTeachersWorkshopId(Guid teacherId)
    {
        logger.LogInformation($"Searching Teacher by Id started. Looking Id = {teacherId}.");

        var teacher = await teacherRepository.GetByFilterNoTracking(t => t.Id == teacherId).SingleOrDefaultAsync().ConfigureAwait(false);

        if (teacher == null)
        {
            throw new ArgumentException(
                nameof(teacherId),
                paramName: $"There are no recors in teachers table with such id - {teacherId}.");
        }

        logger.LogInformation($"Successfully found a Teacher with Id = {teacherId}.");
        var teachersWorkshopId = teacher.WorkshopId;
        logger.LogInformation($"Successfully found WorkshopId - {teachersWorkshopId} for Teacher  with Id = {teacherId}.");
        return teachersWorkshopId ?? Guid.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(Guid id)
    {
        logger.LogDebug("Checking if Teacher exists by Id started. Looking Id = {id}.", id);

        return await teacherRepository.Any(x => x.Id == id);
    }

    private async Task UpdateTeacher()
    {
        try
        {
            await teacherRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Unable to update teacher.");
            throw;
        }
    }
}