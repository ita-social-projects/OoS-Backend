using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Redis
{
    public interface ICacheService
    {
        Task<T> GetOrAdd<T>(string key, Func<Task<T>> newValueFactory);
        
        void Set<T>(string key, T value);

        void ClearCache(string key);

        void Refresh(string key);
    }
}
