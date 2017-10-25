using System;

namespace SharpRetry {
    public class SharpCaller<T> : ISharpCaller<T> {
        private Policy<T> _policy;

        public SharpCaller(Policy<T> policy) {
            _policy = policy;
        }

        public Context<T> Call(Action call, string name = null) {
            return Call(() => {
                call.Invoke();
                return default(T);
            }, name);
        }

        public Context<T> Call(Func<T> call, string name = null) {
            var context = new Context<T>(name);
            var tries = _policy.WaitTimes.Length + 1;
            for (var i = 0; i < tries; i++) {
                if (i > 0) {
                    _policy.OnRetryAction(context);
                }
                else {
                    _policy.BeforeFirstCallAction?.Invoke(context);
                }
                Call(call, context);
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

        private void Call(Func<T> call, Context<T> context) {
            _policy.BeforeEachCallAction?.Invoke(context);
            try {
                var result = call.Invoke();
                context.LastException = null;
                context.Result = result;
                context.IsSuccess = true;
            }
            catch (Exception ex) {
                context.LastException = ex;
                context.IsSuccess = false;
            }
            context.Calls++;
        }
    }
}
