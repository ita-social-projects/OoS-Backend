﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Direction entity.
/// </summary>
public interface IDirectionService
{
    /// <summary>
    /// Add new Direction to the DB.
    /// </summary>
    /// <param name="dto">DirectionDto element.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="DirectionDto"/> that was created.</returns>
    Task<DirectionDto> Create(DirectionDto dto);

    /// <summary>
    /// Get all Direction objects from DB.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="DirectionDto"/> that were found.</returns>
    Task<IEnumerable<DirectionDto>> GetAll();

    /// <summary>
    /// Get Direction objects from DB by filter.
    /// </summary>
    /// <param name="filter">Filter for DirectionDto.</param>
    /// <param name="isAdmins">True, if needs to retrieve information from admin panel.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="DirectionDto"/> that were found.</returns>
    Task<SearchResult<DirectionDto>> GetByFilter(DirectionFilter filter, bool isAdmins);

    /// <summary>
    /// To recieve the Direction object with define id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="DirectionDto"/> that was found.</returns>
    Task<DirectionDto> GetById(long id);

    /// <summary>
    /// To Update our object in DB.
    /// </summary>
    /// <param name="dto">Direction with new properties.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="DirectionDto"/> that was updated.</returns>
    Task<DirectionDto> Update(DirectionDto dto);

    /// <summary>
    /// To delete the object from DB.
    /// </summary>
    /// <param name="id">Key of the Direction in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Result<DirectionDto>> Delete(long id);
}