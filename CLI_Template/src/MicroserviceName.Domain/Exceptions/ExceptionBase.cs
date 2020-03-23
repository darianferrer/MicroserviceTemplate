using System;
using System.Collections.Generic;
using System.Net;

namespace MicroserviceName.Domain.Exceptions
{
    public abstract class ExceptionBase : Exception
    {
        public ExceptionBase(string? message) : base(message)
        {
        }

        public IEnumerable<Error> Errors { get; protected set; } = new List<Error>();

        public HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.InternalServerError;
    }
}
