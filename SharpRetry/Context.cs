using System;

namespace SharpRetry {
    public class Context {
        public bool IsSuccess { get; set; }
        public bool IsFailure => !IsSuccess;
        public int Calls { get; set; }
        public Exception Exception { get; set; }
        public TimeSpan? CallDuration { get; set; }
        public object Userdata { get; set; }
        public object Result { get; set; }

        public T CastResult<T>() {
            return (T)Result;
        }

        public Context() { }

        public Context(object userdata) {
            Userdata = userdata;
        }
    }
}
