using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;

namespace OutOfSchool.WebApi.Util.Transactions
{
    public static class DbContextTypes
    {
        private static readonly Dictionary<DbContextName, Type> ContextTypes = new Dictionary<DbContextName, Type>();

        static DbContextTypes()
        {
            ContextTypes[DbContextName.OutOfSchoolDbContext] = typeof(OutOfSchoolDbContext);
            ContextTypes[DbContextName.FilesDbContext] = typeof(FilesDbContext);
        }

        public static Type GetTypeByName(DbContextName dbContextName)
        {
            return ContextTypes.GetValueOrDefault(dbContextName);
        }
    }
}
