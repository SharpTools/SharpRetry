using System;
using System.Linq;

namespace SharpRetry {
    
    public class Policy : IPolicy {

        public static IPolicy Handle() => new Policy();

        public TimeSpan[] WaitTimes { get; set; } = new TimeSpan[0];
        public Func<Context, bool> RetryFilter { get; set; } = c => c.IsFailure;
        public Action<Context> OnRetryAction { get; set; } = c => { };
        public Action<Context> BeforeFirstCallAction { get; set; }
        public Action<Context> BeforeEachCallAction { get; set; }
        public Action<Context> OnSuccessAction { get; set; }
        public Action<Context> OnFailureAction { get; set; }

        private Policy() {
        }

        public IPolicy BeforeFirstCall(Action<Context> beforeFirstCallAction) {
            BeforeFirstCallAction = beforeFirstCallAction;
            return this;
        }

        public IPolicy BeforeEachCall(Action<Context> beforeEachCallAction) {
            BeforeEachCallAction = beforeEachCallAction;
            return this;
        }
        
        public IPolicy OnSuccess(Action<Context> onSuccessAction) {
            OnSuccessAction = onSuccessAction;
            return this;
        }

        public IPolicy OnFailure(Action<Context> onFailureAction) {
            OnFailureAction = onFailureAction;
            return this;
        }

        public IPolicy RetryOnlyWhen(Func<Context, bool> filter) {
            RetryFilter = filter;
            return this;
        }

        public IPolicy Retry(int times) {
            WaitTimes = Enumerable.Repeat(TimeSpan.Zero, times)
                                                    .ToArray();
            return this;
        }

        public IPolicy RetryAndWait(params TimeSpan[] waitTimes) {
            if (waitTimes == null) {
                waitTimes = new TimeSpan[] { TimeSpan.Zero };
            }
            WaitTimes = waitTimes;
            return this;
        }

        public IPolicy RetryAndWaitInSeconds(params int[] waitInSeconds) {
            if (waitInSeconds == null) {
                waitInSeconds = new int[] { 0 };
            }
            WaitTimes = waitInSeconds.Select(s => TimeSpan.FromSeconds(s))
                                                          .ToArray();
            return this;
        }

        public IPolicy OnRetry(Action<Context> onRetryAction) {
            OnRetryAction = onRetryAction;
            return this;
        }

        public ISharpCaller BuildCaller() {
            return new SharpCaller(this);
        }
    }
}
