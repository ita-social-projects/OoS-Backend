using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Redis
{
    public interface ICacheService
    {
        Task<T> Get<T>(string key);
        
        Task Set<T>(string key, T value);

        Task ClearCache(string key);

        Task Refresh(string key);
    }
}
