using System;

namespace SharpRetry {
    public interface IPolicy {
        IPolicy BeforeEachCall(Action<Context> beforeEachCallAction);
        IPolicy BeforeFirstCall(Action<Context> beforeFirstCallAction);
        ISharpCaller BuildCaller();
        IPolicy OnRetry(Action<Context> onRetryAction);
        IPolicy OnSuccess(Action<Context> onSuccessAction);
        IPolicy OnFailure(Action<Context> onFailureAction);
        IPolicy Retry(int times);
        IPolicy RetryAndWait(params TimeSpan[] waitTimes);
        IPolicy RetryAndWaitInSeconds(params int[] waitInSeconds);
        IPolicy RetryOnlyWhen(Func<Context, bool> filter);
    }
}