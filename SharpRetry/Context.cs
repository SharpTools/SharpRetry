using System;

namespace SharpRetry {
    public class Context<T> {
        public bool IsFailure => !IsSuccess;
        public bool IsSuccess { get; set; }
        public string CallName { get; set; }
        public int Calls { get; set; }
        public Exception LastException { get; set; }
        public T Result { get; set; }

        public Context() { }

        public Context(string callName) {
            CallName = callName;
        }
    }
}
