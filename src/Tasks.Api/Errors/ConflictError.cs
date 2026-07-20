using FluentResults;

namespace Tasks.Api.Errors {
    public class ConflictError : Error {
        public ConflictError(string message) : base(message) { }
    }
}
