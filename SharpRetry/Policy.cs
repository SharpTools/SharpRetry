using System;
using System.Linq;

namespace SharpRetry {

    public static class Policy {
        public static IPolicy<T> HandleResult<T>() => new Policy<T>();
        public static IPolicy<NoResult> Handle() => new Policy<NoResult>();
    }

    public class Policy<T> : IPolicy<T> {
        public TimeSpan[] WaitTimes { get; set; } = new TimeSpan[0];
        public Func<Context<T>, bool> RetryFilter { get; set; } = c => c.IsFailure;
        public Action<Context<T>> OnRetryAction { get; set; } = c => { };

        public Action<Context<T>> BeforeFirstCallAction { get; set; }
        public Action<Context<T>> BeforeEachCallAction { get; set; }
        public Action<Context<T>> OnSuccessAction { get; set; }
        public Action<Context<T>> OnFailureAction { get; set; }

        public static Policy<T> Handle() => new Policy<T>();

        public Policy() {
        }

        public IPolicy<T> BeforeFirstCall(Action<Context<T>> beforeFirstCallAction) {
            BeforeFirstCallAction = beforeFirstCallAction;
            return this;
        }

        public IPolicy<T> BeforeEachCall(Action<Context<T>> beforeEachCallAction) {
            BeforeEachCallAction = beforeEachCallAction;
            return this;
        }
        
        public IPolicy<T> OnSuccess(Action<Context<T>> onSuccessAction) {
            OnSuccessAction = onSuccessAction;
            return this;
        }

        public IPolicy<T> OnFailure(Action<Context<T>> onFailureAction) {
            OnFailureAction = onFailureAction;
            return this;
        }

        public IPolicy<T> RetryOnlyWhen(Func<Context<T>, bool> filter) {
            RetryFilter = filter;
            return this;
        }

        public IPolicy<T> Retry(int times) {
            WaitTimes = Enumerable.Repeat(TimeSpan.Zero, times)
                                                    .ToArray();
            return this;
        }

        public IPolicy<T> RetryAndWait(params TimeSpan[] waitTimes) {
            if (waitTimes == null) {
                waitTimes = new TimeSpan[] { TimeSpan.Zero };
            }
            WaitTimes = waitTimes;
            return this;
        }

        public IPolicy<T> RetryAndWaitInSeconds(params int[] waitInSeconds) {
            if (waitInSeconds == null) {
                waitInSeconds = new int[] { 0 };
            }
            WaitTimes = waitInSeconds.Select(s => TimeSpan.FromSeconds(s))
                                                          .ToArray();
            return this;
        }

        public IPolicy<T> OnRetry(Action<Context<T>> onRetryAction) {
            OnRetryAction = onRetryAction;
            return this;
        }

        public ISharpCaller<T> BuildCaller() {
            return new SharpCaller<T>(this);
        }
    }

    public class NoResult {
        public static NoResult Instance => new NoResult();
    }
}
