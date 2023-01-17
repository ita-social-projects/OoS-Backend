﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface ICodeficatorRepository : IEntityRepository<long, CATOTTG>
{
    /// <summary>
    /// Get elements pair values (Id, Name) by a specific filter.
    /// </summary>
    /// <param name="predicate">Filter with key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{KeyValuePair}"/> that contains elements' Id and Name.</returns>
    public Task<IEnumerable<KeyValuePair<long, string>>> GetNamesByFilter(Expression<Func<CATOTTG, bool>> predicate);

    /// <summary>
    /// Get elements' list by a part of name.
    /// </summary>
    /// <param name="namePart">Part of name for search.</param>
    /// <param name="categories">Categories for search.</param>
    /// <returns>The task result contains a <see cref="List{CodeficatorAddressDto}"/> that contains elements' full addresses.</returns>
    public Task<List<CodeficatorAddressDto>> GetFullAddressesByPartOfName(string namePart, string categories = default);

    /// <summary>
    /// Get the subsettlements of current settlements by CATOTTG.
    /// </summary>
    /// <param name="catottgId">CATOTTG id</param>
    /// <returns>The task result contains a <see cref="List{long}"/> that contains subsettlements ids.</returns>
    public Task<List<long>> GetSubSettlementsIds(long catottgId);
}