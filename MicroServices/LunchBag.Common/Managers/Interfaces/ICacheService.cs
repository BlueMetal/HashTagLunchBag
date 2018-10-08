using System;
using System.Threading.Tasks;

namespace LunchBag.Common.Managers
{
    public interface ICacheService
    {
        Task<T> Get<T>(string key);

        Task Set(string key, object value, TimeSpan duration);

        Task Set(string key, object value);
    }
}