using FluentResults;
using System.Reflection.Metadata.Ecma335;

namespace Tasks.Api.Errors {
    public class NotFoundError : Error {
        public NotFoundError(string message) : base(message) {
        }
    }
}
