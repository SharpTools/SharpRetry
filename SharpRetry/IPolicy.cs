using System;

namespace SharpRetry {
    public interface IPolicy<T> {
        IPolicy<T> BeforeEachCall(Action<Context<T>> beforeEachCallAction);
        IPolicy<T> BeforeFirstCall(Action<Context<T>> beforeFirstCallAction);
        ISharpCaller<T> BuildCaller();
        IPolicy<T> OnRetry(Action<Context<T>> onRetryAction);
        IPolicy<T> OnSuccess(Action<Context<T>> onSuccessAction);
        IPolicy<T> OnFailure(Action<Context<T>> onFailureAction);
        IPolicy<T> Retry(int times);
        IPolicy<T> RetryAndWait(params TimeSpan[] waitTimes);
        IPolicy<T> RetryAndWaitInSeconds(params int[] waitInSeconds);
        IPolicy<T> RetryOnlyWhen(Func<Context<T>, bool> filter);
    }
}