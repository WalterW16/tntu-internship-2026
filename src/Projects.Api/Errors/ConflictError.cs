using FluentResults;

namespace Projects.Api.Errors {
    public class ConflictError : Error {
        public ConflictError(string message) : base(message) { }
    }
}
