using System;
using System.Threading.Tasks;

namespace SharpRetry {
    public interface ISharpCaller<T> {
        Task<Context<T>> CallAsync(Func<Task> call, string name = null);
        Task<Context<T>> CallAsync(Func<Task<T>> asyncCall, string name = null);
    }
}