using System;
using System.Threading.Tasks;

namespace SharpRetry {
    public interface ISharpCaller {
        Task<Context> CallAsync(Func<Task> asyncCall, string name = null);
        Task<Context> CallAsync<T>(Func<Task<T>> asyncCall, string name = null);
    }
}