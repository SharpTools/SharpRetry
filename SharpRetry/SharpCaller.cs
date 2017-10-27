using System;
using System.Threading.Tasks;

namespace SharpRetry {
    public class SharpCaller : ISharpCaller {
        private Policy _policy;

        public SharpCaller(Policy policy) {
            _policy = policy;
        }

        public async Task<Context> CallAsync(Func<Task> asyncCall, string name = null) {
            return await CallAsync<NoReturn>(async () => {
                await asyncCall.Invoke();
                return null;
            }, name);
        }

        public async Task<Context> CallAsync<T>(Func<Task<T>> asyncCall, string name = null) {
            var context = new Context(name);
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

        private async Task CallAsync<T>(Func<Task<T>> call, Context context) {
            context.Calls++;
            _policy.BeforeEachCallAction?.Invoke(context);
            try {
                context.CallBegin = DateTime.Now;
                var result = await call.Invoke();
                context.Exception = null;
                context.Result = result;
                context.IsSuccess = true;
            }
            catch (Exception ex) {
                context.Exception = ex;
                context.IsSuccess = false;
            }
            context.CallEnd = DateTime.Now;
        }

        private class NoReturn {}
    }
}
