using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MicroserviceName.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Severity = MicroserviceName.Domain.Exceptions.Severity;

namespace MicroserviceName.Host.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (errors, statusCode) = HandleError(ex);
                await WriteErrorResponseAsync(context, errors, statusCode);
            }
        }

        private static (IEnumerable<Error> errors, int statusCode) HandleError(Exception ex)
        {
            ICollection<Error> errors;
            int statusCode;

            if (ex.GetBaseException() is ExceptionBase baseException)
            {
                statusCode = (int)baseException.StatusCode;
                errors = baseException.Errors.ToList();
            }
            else if (ex is ValidationException validationException)
            {
                errors = validationException.Errors.Select(e => new Error(Severity.Correctable, e.ErrorCode, e.ErrorMessage)).ToList();
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (ex is AggregateException aggregateException)
            {
                var responses = aggregateException.InnerExceptions.Select(HandleError);
                statusCode = StatusCodes.Status500InternalServerError;
                errors = responses.SelectMany(e => e.errors).ToList();
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                errors = new List<Error>
                {
                    new Error(Severity.Unexpected, ex.GetType().Name, ex.Message),
                };
                GetInnerExceptionIfExists(ex, errors);
            }

            return (errors, statusCode);
        }

        private static void GetInnerExceptionIfExists(Exception ex, ICollection<Error> errors)
        {
            if (ex.InnerException != null)
            {
                var (innerErrors, _) = HandleError(ex.InnerException);
                foreach (var error in innerErrors)
                {
                    errors.Add(error);
                }
            }
        }

        private static Task WriteErrorResponseAsync(HttpContext context, IEnumerable<Error> errors, int statusCode = StatusCodes.Status500InternalServerError)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
            return response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Errors = errors.ToList(),
            }));
        }
    }
}
