using FluentResults;

namespace Tasks.Api.Errors {
    public class BadGatewayError : Error {
        public BadGatewayError(string message) : base(message) { }
    }
}
