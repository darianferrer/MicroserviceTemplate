using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MicroserviceName.Domain.Exceptions
{
    public class ServiceClientException : ExceptionBase
    {
        public ServiceClientException(IEnumerable<Error> errors) : base(errors.FirstOrDefault()?.Detail)
        {
            Errors = errors;
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ServiceClientException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
        {
            Errors = new[]
            {
                new Error(Severity.Unexpected, typeof(ServiceClientException).Name, message),
            };
            StatusCode = statusCode;
        }
    }
}
