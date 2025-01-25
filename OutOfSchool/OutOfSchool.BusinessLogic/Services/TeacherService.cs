using AutoMapper;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Teachers;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Teacher entity.
/// </summary>
public class TeacherService : ITeacherService
{
    private readonly ISensitiveEntityRepositorySoftDeleted<Teacher> teacherRepository;
    private readonly IEntityCoverImageInteractionService<Teacher> teacherImagesService;
    private readonly ILogger<TeacherService> logger;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeacherService"/> class.
    /// </summary>
    /// <param name="teacherRepository">Repository for Teacher entity.</param>
    /// <param name="teacherImagesService">Teacher images mediator.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="mapper">Mapper.</param>
    public TeacherService(ISensitiveEntityRepositorySoftDeleted<Teacher> teacherRepository, IEntityCoverImageInteractionService<Teacher> teacherImagesService, ILogger<TeacherService> logger, IMapper mapper)
    {
        this.teacherRepository = teacherRepository ?? throw new ArgumentNullException(nameof(teacherRepository));
        this.teacherImagesService = teacherImagesService ?? throw new ArgumentNullException(nameof(teacherImagesService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public async Task<TeacherCreationResultDto> Create(Guid workshopId, TeacherCreateDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogDebug("Teacher creating was started");

        var teacher = mapper.Map<Teacher>(dto);
        teacher.Id = Guid.Empty;
        teacher.WorkshopId = workshopId;

        var newTeacher = await teacherRepository.Create(teacher).ConfigureAwait(false);

        Result<string> uploadingResult = null;
        if (dto.CoverImage != null)
        {
            uploadingResult = await teacherImagesService.AddCoverImageAsync(newTeacher, dto.CoverImage).ConfigureAwait(false);
            if (uploadingResult.Succeeded)
            {
                teacher.CoverImageId = uploadingResult.Value;
                await UpdateTeacher().ConfigureAwait(false);
            }
        }

        logger.LogDebug("Teacher with Id = {Id} created successfully", newTeacher.Id);

        return new TeacherCreationResultDto
        {
            Teacher = mapper.Map<TeacherDto>(newTeacher),
            UploadingAvatarImageResult = uploadingResult?.OperationResult,
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TeacherDto>> GetAll()
    {
        logger.LogDebug("Getting all Teachers started");

        var teachers = await teacherRepository.GetAll().ConfigureAwait(false);

        logger.LogDebug("All {Count} records were successfully received from the Teacher table", teachers.Count());

        return teachers.Select(teacher => mapper.Map<TeacherDto>(teacher)).ToList();
    }

    /// <inheritdoc/>
    public async Task<TeacherDto> GetById(Guid id)
    {
        logger.LogDebug("Getting Teacher by Id started. Looking Id = {Id}", id);

        var teacher = await teacherRepository.GetById(id).ConfigureAwait(false);

        if (teacher == null)
        {
            throw new ArgumentException(
                nameof(id),
                paramName: $"There are no recors in teachers table with such id - {id}.");
        }

        logger.LogDebug("Got a Teacher with Id = {Id}", id);

        return mapper.Map<TeacherDto>(teacher);
    }

    /// <inheritdoc/>
    public async Task<TeacherUpdateResultDto> Update(TeacherUpdateDto dto)
    {
        _ = dto ?? throw new ArgumentNullException(nameof(dto));
        logger.LogDebug("Updating Teacher with Id = {Id} started", dto.Id);

        var teacher = await teacherRepository.GetById(dto.Id).ConfigureAwait(false);

        mapper.Map(dto, teacher);

        var changingAvatarResult = await teacherImagesService.ChangeCoverImageAsync(teacher, dto.CoverImageId, dto.CoverImage).ConfigureAwait(false);

        await UpdateTeacher().ConfigureAwait(false);

        return new TeacherUpdateResultDto
        {
            Teacher = mapper.Map<TeacherDto>(teacher),
            UploadingAvatarImageResult = changingAvatarResult?.UploadingResult?.OperationResult,
        };
    }

    /// <inheritdoc/>
    public async Task Delete(Guid id)
    {
        logger.LogDebug("Deleting Teacher with Id = {Id} started", id);

        var entity = await teacherRepository.GetById(id).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(entity.CoverImageId))
        {
            await teacherImagesService.RemoveCoverImageAsync(entity).ConfigureAwait(false);
        }

        try
        {
            await teacherRepository.Delete(entity).ConfigureAwait(false);

            logger.LogDebug("Teacher with Id = {Id} successfully deleted", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError("Deleting Teacher with Id = {Id} failed", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Guid> GetTeachersWorkshopId(Guid teacherId)
    {
        logger.LogDebug("Searching Teacher by Id started. Looking Id = {Id}", teacherId);

        var teacher = await teacherRepository.GetByFilterNoTracking(t => t.Id == teacherId).SingleOrDefaultAsync().ConfigureAwait(false);

        if (teacher == null)
        {
            throw new ArgumentException(
                nameof(teacherId),
                paramName: $"There are no recors in teachers table with such id - {teacherId}.");
        }

        logger.LogDebug("Successfully found a Teacher with Id = {Id}", teacherId);
        var teachersWorkshopId = teacher.WorkshopId;
        logger.LogDebug("Successfully found WorkshopId - {WorkshopId} for Teacher  with Id = {Id}", teachersWorkshopId, teacherId);
        return teachersWorkshopId ?? Guid.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(Guid id)
    {
        logger.LogDebug("Checking if Teacher exists by Id started. Looking Id = {Id}", id);

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
            logger.LogError(ex, "Unable to update teacher");
            throw;
        }
    }
}