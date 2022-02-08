using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Redis
{
    public interface ICacheService
    {
        Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> newValueFactory);
        
        Task SetAsync<T>(string key, T value);

        Task ClearCacheAsync(string key);

        Task RefreshAsync(string key);
    }
}
