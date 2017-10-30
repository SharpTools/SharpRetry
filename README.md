# SharpRetry

SharpRetry is a simple and powerful library to create retry policies for your .net application. Create a policy once and use it everywhere in your system to make async calls in an easy and unit testable way.

# Nuget
    Install-Package SharpRetry -pre

# How to use it

## Step 1: Create a caller with some policy


Ex: Retry 3 times in case of exception
```cs
ISharpCaller caller = Policy.Handle()
                            .Retry(3)
                            .BuildCaller();
```

## Step 2: User your caller to make async calls

```cs

Context result = await caller.CallAsync(() => _httpClient.GetAsync("https://www.foo.com"));
```

# Configuration

You can make use of many callbacks to control your policy workflow. Here is a complex example:

Ex:

``` cs
var caller = Policy.Handle()
                   .BeforeFirstCall(c => Log("First Try"))
                   .BeforeEachCall(c => Log("Trying: " + c.Calls))
                   .RetryOnlyWhen(c => c.CastResult<HttpResponseMessage>().StatusCode == HttpStatusCode.ServiceUnavailable)
                   .RetryAndWaitInSeconds(1,2,4)
                   .OnRetry(c => Log("Retry: " + c.Calls)
                   .OnSuccess(c => Log("Success"))
                   .OnFailure(c => Log("Final failure"))
                   .BuildCaller();
```

## The Context

You have access to the call `Context` on every configuration method and it is also the result of your call. Here is its content:


| Property    | Description|
| ---------   | -----------|
| IsSuccess   | true if the call passed the policy.
| Calls       | the current call number or the total number of calls if the policy ended.
| Exception   | the last exception 
| CallDuration| the last call duration
| Userdata    | any user data you want to pass to the policy pipeline
| Result      | the final result of your original call 

# Exception handling:

The ISharpCaller call will always retry on any exception, unless you use the method `RetryOnlyWhen(context => {})` to customize what is considered a failure.

# Other Examples:

## Automatically renew an API access token

```cs
    _caller = Policy.Handle()
                    .RetryOnlyWhen(c => {
                        return c.CastResult<HttpResponseMessage>()
                                .StatusCode == HttpStatusCode.Unauthorized;
                    })
                    .OnRetry(c => {
                        var client = (ApiClient)c.Userdata;
                        client.RefreshToken();
                    })
                    .Retry(1)
                    .BuildCaller();
```

## Log every webservice call

```cs
_caller = Policy.Handle()
                .BeforeEachCall(c => Console.Write($"Call {c.Userdata} Try: [{c.Calls}]"))
                .RetryOnlyWhen(c => !c.CastResult<HttpResponseMessage>().IsSuccessStatusCode)
                .OnRetry(c => Console.WriteLine($" Error: {c.CastResult<HttpResponseMessage>().StatusCode}"))
                .OnSuccess(c => Console.WriteLine(" Success"))
                .OnFailure(c => Console.WriteLine(" Failure"))
                .RetryAndWaitInSeconds(1, 2, 4)
                .BuildCaller();

var result = await _caller.CallAsync(() => client.PostAsync(url, content), "Some Method");
```

# Remarks

This project is heavily inspired on [Polly](https://github.com/App-vNext/Polly), a much bigger and complex resilience and transient-fault-handling library. Go check them out :)