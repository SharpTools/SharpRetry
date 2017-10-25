using System;
using System.Threading.Tasks;

namespace SharpRetry {
    public interface ISharpCaller<T> {
        Context<T> Call(Action call, string name = null);
        Context<T> Call(Func<T> call, string name = null);
    }
}