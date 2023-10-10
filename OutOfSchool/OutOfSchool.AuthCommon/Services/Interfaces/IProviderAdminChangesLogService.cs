﻿using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Services.Interfaces;

public interface IProviderAdminChangesLogService
{
    /// <summary>
    /// Create changes log for the given <see cref="ProviderAdmin"/> entity.
    /// </summary>
    /// <param name="entity">Modified <see cref="ProviderAdmin"/> entity.</param>
    /// <param name="userId">ID of user that performs the change.</param>
    /// <param name="operationType">Type of the change operation.</param>
    /// <param name="propertyName">Name of the property that is changing.</param>
    /// <param name="oldValue">Old value of the property that is changing.</param>
    /// <param name="newValue">New value of the property that is changing.</param>
    /// <returns>Number of the added log records.</returns>
    Task<int> SaveChangesLogAsync(
        ProviderAdmin entity,
        string userId,
        OperationType operationType,
        string propertyName,
        string? oldValue,
        string? newValue);
}