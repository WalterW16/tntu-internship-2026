using FluentResults;
using System.Reflection.Metadata.Ecma335;

namespace Projects.Api.Errors {
    public class NotFoundError : Error{
        public NotFoundError(string message) : base(message){            
        }
    }
}
