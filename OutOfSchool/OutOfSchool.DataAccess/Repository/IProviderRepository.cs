﻿using System;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IProviderRepository : ISensitiveEntityRepository<Provider>, IExistable<Provider>
{
    bool ExistsUserId(string id);
    //Task<Provider> GetByEmail(string email);
}