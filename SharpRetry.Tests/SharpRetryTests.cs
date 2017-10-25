using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SharpRetry.Tests {
    public class SharpRetryTests {

        private ExternalCall _client;

        public SharpRetryTests() {
            _client = new ExternalCall();
        }

        [Fact]
        public void Should_retry_1_time() {
            var caller = Policy.HandleResult<string>()
                               .RetryOnlyWhen(c => c.Result == "0")
                               .Retry(1)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Equal("1", response.Result);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public void Should_call_onRetry() {
            var onRetry = 0;
            var caller = Policy.HandleResult<string>()
                               .RetryOnlyWhen(c => c.Result == "0")
                               .Retry(1)
                               .OnRetry(c => { onRetry++; })
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Equal("1", response.Result);
            Assert.Equal(1, onRetry);
        }

        [Fact]
        public void Should_call_beforeEachCall() {
            var beforeEachCall = 0;
            var caller = Policy.HandleResult<string>()
                               .RetryOnlyWhen(c => true)
                               .BeforeEachCall(c => beforeEachCall++)
                               .Retry(3)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Equal(4, beforeEachCall);
        }

        [Fact]
        public void Should_call_beforeFirstCall() {
            var beforeFirstCall = 0;
            var caller = Policy.HandleResult<string>()
                               .RetryOnlyWhen(c => true)
                               .BeforeFirstCall(c => beforeFirstCall++)
                               .Retry(3)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Equal(1, beforeFirstCall);
        }

        [Fact]
        public void Should_call_onSuccess() {
            var onSuccess = 0;
            var caller = Policy.HandleResult<string>()
                               .RetryOnlyWhen(c => c.Calls == 4)
                               .OnSuccess(c => onSuccess++)
                               .Retry(3)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Equal(1, onSuccess);
        }

        [Fact]
        public void Should_fail_on_undesirable_result() {
            var caller = Policy.HandleResult<string>()
                               .RetryOnlyWhen(c => c.Result != null)
                               .Retry(1)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Equal("1", response.Result);
            Assert.True(response.IsFailure);
            Assert.Equal(2, response.Calls);
        }

        [Fact]
        public void Should_fail_on_exception() {
            var exception = new Exception();
            _client.BeforeCallAction = i => throw exception;

            var caller = Policy.HandleResult<string>()
                               .Retry(1)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(1));
            Assert.Null(response.Result);
            Assert.Equal(exception, response.LastException);
            Assert.True(response.IsFailure);
        }

        [Fact]
        public void Should_recover_from_exception() {
            _client.BeforeCallAction = call => {
                if (call == 0) {
                    throw new Exception();
                }
            };

            var caller = Policy.HandleResult<string>()
                               .Retry(1)
                               .BuildCaller();

            var response = caller.Call(() => _client.Request(0));
            Assert.Equal("1", response.Result);
            Assert.Null(response.LastException);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public void Should_retry_1_time_for_actions() {
            var caller = Policy.Handle()
                               .Retry(1)
                               .BuildCaller();

            var response = caller.Call(() => _client.RequestAction(1));
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public void All_together() {
            var beforeFirstCall = 0;
            var beforeEachCall = 0;
            var retries = 0;
            var success = 0;
            var failure = 0;
            var caller = Policy.HandleResult<string>()
                               .BeforeFirstCall(c => beforeFirstCall++)
                               .BeforeEachCall(c=> beforeEachCall++)
                               .RetryOnlyWhen(c => c.Calls <= 3)
                               .RetryAndWaitInSeconds(0,1,1)
                               .OnRetry(c => retries++)
                               .OnSuccess(c => success++)
                               .OnFailure(c => failure++)
                               .BuildCaller();

            var response = caller.Call(() => _client.RequestAction(1));
            Assert.True(response.IsSuccess);
            Assert.Equal(beforeFirstCall, 1);
            Assert.Equal(beforeEachCall, 4);
            Assert.Equal(retries, 3);
            Assert.Equal(success, 1);
        }
    }

    public class ExternalCall {

        public int Calls { get; set; }

        public Action<int> BeforeCallAction { get; set; } = call => { };
        
        public string Request(int foo) {
            Calls++;
            BeforeCallAction(Calls-1);
            return (Calls - 1).ToString();
        }

        public void RequestAction(int foo) {
            Calls++;
            BeforeCallAction(Calls - 1);
        }

        public async Task<string> RequestAsync(int foo) {
            return await Task.FromResult(Request(foo));
        }
    }
}
