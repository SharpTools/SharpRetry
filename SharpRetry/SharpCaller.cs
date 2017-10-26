using System;
using System.Threading.Tasks;

namespace SharpRetry {
    public class SharpCaller<T> : ISharpCaller<T> {
        private Policy<T> _policy;

        public SharpCaller(Policy<T> policy) {
            _policy = policy;
        }

        public async Task<Context<T>> CallAsync(Func<Task> call, string name = null) {
            return await CallAsync(async () => {
                await call.Invoke();
                return default(T);
            }, name);
        }

        public async Task<Context<T>> CallAsync(Func<Task<T>> asyncCall, string name = null) {
            var context = new Context<T>(name);
            var tries = _policy.WaitTimes.Length + 1;
            for (var i = 0; i < tries; i++) {
                if (i > 0) {
                    await Task.Delay(_policy.WaitTimes[i - 1]);
                    _policy.OnRetryAction(context);
                }
                else {
                    _policy.BeforeFirstCallAction?.Invoke(context);
                }
                await CallAsync(asyncCall, context);
                if (!_policy.RetryFilter(context)) {
                    break;
                }
                context.IsSuccess = false;
            }
            if (context.IsSuccess) {
                _policy.OnSuccessAction?.Invoke(context);
            }
            else {
                _policy.OnFailureAction?.Invoke(context);
            }
            return context;
        }

        private async Task CallAsync(Func<Task<T>> call, Context<T> context) {
            _policy.BeforeEachCallAction?.Invoke(context);
            try {
                var result = await call.Invoke();
                context.Exception = null;
                context.Result = result;
                context.IsSuccess = true;
            }
            catch (Exception ex) {
                context.Exception = ex;
                context.IsSuccess = false;
            }
            context.Calls++;
        }
    }
}
