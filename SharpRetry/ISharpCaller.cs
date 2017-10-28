using System;
using System.Threading.Tasks;

namespace SharpRetry {
    public interface ISharpCaller {
        Task<Context> CallAsync(Func<Task> asyncCall, object userdata = null);
        Task<Context> CallAsync<T>(Func<Task<T>> asyncCall, object userdata = null);
    }
}