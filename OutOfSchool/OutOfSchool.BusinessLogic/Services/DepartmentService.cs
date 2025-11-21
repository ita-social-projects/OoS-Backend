using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Department;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

public class DepartmentService(
    ILogger<PositionService> logger,
    ICurrentUserService currentUserService,
    IDepartmentRepository departmentRepository
) : IDepartmentService
{
    public async Task<DepartmentDto> CreateAsync(DepartmentCreateUpdateDto createDto, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId), new DeputyDirectorRights(providerId));
        
        // Validate and prepare Contacts
        ValidateAndPrepareContacts(createDto);
        
        // Validate uniqueness if ParentDepartmentId is not null
        if (createDto.ParentDepartmentId.HasValue)
        {
            await ValidateDepartmentUniquenessAsync(createDto, createDto.ParentDepartmentId.Value, null).ConfigureAwait(false);
        }
        
        var department = createDto.ToModel();
        department.ActiveFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        department.ActiveTo = new DateOnly(2999, 12, 31);
        var createdDepartment = await departmentRepository.Create(department);
        logger.LogDebug("Created department with id: {DepartmentId}", createdDepartment.Id);
        return createdDepartment.ToDto();
    }

    public async Task<SearchResult<DepartmentDto>> GetByFilter(Guid providerId, DepartmentFilter filter)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));

        logger.LogInformation("Getting all Departments started (by filter)");

        filter ??= new DepartmentFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var predicate = PredicateBuilder.True<Department>();

        // Filter by FullName, ShortName, or Abbreviation
        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            predicate = predicate.And(d =>
                d.FullName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase) ||
                d.ShortName != null && d.ShortName.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase) ||
                d.Abbreviation != null && d.Abbreviation.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));
        }

        // Filter out deleted departments
        predicate = predicate.And(d => !d.IsDeleted);

        // Define sorting
        var sortPredicate = SortExpressionBuild(filter);

        var count = await departmentRepository.Count(whereExpression: predicate).ConfigureAwait(false);

        var departments = await departmentRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                whereExpression: predicate,
                orderBy: sortPredicate)
            .IncludeContactsWithCodeficatorHierarchy()
            .ToListAsync()
            .ConfigureAwait(false);

        logger.LogInformation("Retrieved {DepartmentsCount} departments", departments.Count);

        var result = new SearchResult<DepartmentDto>
        {
            TotalAmount = count,
            Entities = departments.ToDto(),
        };

        return result;
    }
    
    private static void ValidateAndPrepareContacts(DepartmentCreateUpdateDto dto)
    {
        if (dto.Contacts == null || dto.Contacts.Count == 0)
        {
            throw new InvalidOperationException("At least one contact is required.");
        }
        
        // Remove duplicates
        var unique = dto.Contacts.Distinct().ToList();
        
        // Validate required fields
        foreach (var contact in unique)
        {
            if (contact.Address == null)
            {
                throw new InvalidOperationException("Address must be specified for each contact.");
            }
            
            if (contact.Phones == null || contact.Phones.Count == 0)
            {
                throw new InvalidOperationException("At least one phone number must be specified for each contact.");
            }
        }
        
        // Validate default count
        var defaultCount = unique.Count(c => c.IsDefault);
        if (defaultCount == 0)
        {
            unique[0].IsDefault = true;
        }
        else if (defaultCount > 1)
        {
            throw new InvalidOperationException($"Exactly one Contact must be default, but found {defaultCount}.");
        }
        
        dto.Contacts = unique;
    }

    public async Task DeleteAsync(Guid id, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));
        var department = await GetDepartmentAsync(id, providerId);
        if (department is null)
        {
            return;
        }

        // Check for active child departments (dependencies)
        var hasActiveChildren = await departmentRepository.Get(
            whereExpression: d => d.ParentDepartmentId == id && !d.IsDeleted)
            .AnyAsync()
            .ConfigureAwait(false);

        if (hasActiveChildren)
        {
            throw new InvalidOperationException("Cannot archive department. Active child departments exist.");
        }

        logger.LogInformation("Deleting department with id: {DepartmentId} for provider {ProviderId}", department.Id, providerId);
        await departmentRepository.Delete(department);
    }

    public async Task<DepartmentDto> GetByIdAsync(Guid id, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));
        var department = await GetDepartmentAsync(id, providerId);
        return department?.ToDto();
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, DepartmentCreateUpdateDto updateDto, Guid providerId)
    {
        await currentUserService.UserHasRights(new ProviderRights(providerId));
        var existingDepartment = await GetDepartmentAsync(id, providerId);
        if (existingDepartment is null)
        {
            throw new KeyNotFoundException($"Department with id {id} not found.");
        }

        // Validate and prepare Contacts if provided
        if (updateDto.Contacts != null)
        {
            ValidateAndPrepareContacts(updateDto);
        }

        // Validate uniqueness if ParentDepartmentId is not null
        // Use the ParentDepartmentId from updateDto if provided (for updating parent), otherwise use existing
        // This ensures we validate against the correct parent (new parent if changed, existing if not)
        var parentDepartmentId = updateDto.ParentDepartmentId ?? existingDepartment.ParentDepartmentId;
        if (parentDepartmentId.HasValue)
        {
            await ValidateDepartmentUniquenessAsync(updateDto, parentDepartmentId.Value, id).ConfigureAwait(false);
        }

        var updatedDepartment = await departmentRepository.Update(updateDto.SetToModel(existingDepartment));
        return updatedDepartment.ToDto();
    }

    private async Task<Department> GetDepartmentAsync(Guid id, Guid providerId)
    {
        var department = await departmentRepository.Get(whereExpression: x => x.Id == id)
            .IncludeContactsWithCodeficatorHierarchy()
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        return department;
    }

    private async Task ValidateDepartmentUniquenessAsync(DepartmentCreateUpdateDto dto, Guid parentDepartmentId, Guid? excludeDepartmentId)
    {
        // Build predicate to find departments with the same ParentDepartmentId
        var predicate = PredicateBuilder.True<Department>()
            .And(d => d.ParentDepartmentId == parentDepartmentId)
            .And(d => !d.IsDeleted);

        // Exclude current department if updating
        if (excludeDepartmentId.HasValue)
        {
            predicate = predicate.And(d => d.Id != excludeDepartmentId.Value);
        }

        // Get all existing departments with the same parent
        var existingDepartments = await departmentRepository
            .Get(whereExpression: predicate)
            .ToListAsync()
            .ConfigureAwait(false);

        // Check FullName uniqueness
        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            var duplicateFullName = existingDepartments
                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.FullName) &&
                    d.FullName.Equals(dto.FullName, StringComparison.OrdinalIgnoreCase));
            if (duplicateFullName != null)
            {
                throw new InvalidOperationException(
                    $"Department with FullName '{dto.FullName}' already exists under the same parent department.");
            }
        }

        // Check Abbreviation uniqueness
        if (!string.IsNullOrWhiteSpace(dto.Abbreviation))
        {
            var duplicateAbbreviation = existingDepartments
                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.Abbreviation) &&
                    d.Abbreviation.Equals(dto.Abbreviation, StringComparison.OrdinalIgnoreCase));
            if (duplicateAbbreviation != null)
            {
                throw new InvalidOperationException(
                    $"Department with Abbreviation '{dto.Abbreviation}' already exists under the same parent department.");
            }
        }

        // Check ShortName uniqueness
        if (!string.IsNullOrWhiteSpace(dto.ShortName))
        {
            var duplicateShortName = existingDepartments
                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.ShortName) &&
                    d.ShortName.Equals(dto.ShortName, StringComparison.OrdinalIgnoreCase));
            if (duplicateShortName != null)
            {
                throw new InvalidOperationException(
                    $"Department with ShortName '{dto.ShortName}' already exists under the same parent department.");
            }
        }

        // Check GenitiveName uniqueness
        if (!string.IsNullOrWhiteSpace(dto.GenitiveName))
        {
            var duplicateGenitiveName = existingDepartments
                .FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.GenitiveName) &&
                    d.GenitiveName.Equals(dto.GenitiveName, StringComparison.OrdinalIgnoreCase));
            if (duplicateGenitiveName != null)
            {
                throw new InvalidOperationException(
                    $"Department with GenitiveName '{dto.GenitiveName}' already exists under the same parent department.");
            }
        }
    }

    private static Dictionary<Expression<Func<Department, object>>, SortDirection> SortExpressionBuild(
        DepartmentFilter filter)
    {
        var sortExpression = new Dictionary<Expression<Func<Department, object>>, SortDirection>();

        switch (filter.FilterByProperty?.ToLower())
        {
            case string property when property.Equals(nameof(Department.FullName).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(d => d.FullName, filter.Order ? SortDirection.Ascending : SortDirection.Descending);
                break;

            case string property when property.Equals(nameof(Department.ShortName).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(d => d.ShortName, filter.Order ? SortDirection.Ascending : SortDirection.Descending);
                break;

            case string property when property.Equals(nameof(Department.DepartmentType).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(d => d.DepartmentType, filter.Order ? SortDirection.Ascending : SortDirection.Descending);
                break;

            case string property when property.Equals(nameof(Department.CreatedAt).ToLower(), StringComparison.OrdinalIgnoreCase):
                sortExpression.Add(d => d.CreatedAt, filter.Order ? SortDirection.Ascending : SortDirection.Descending);
                break;

            default:
                sortExpression.Add(d => d.CreatedAt, SortDirection.Descending);
                break;
        }

        return sortExpression;
    }
}
