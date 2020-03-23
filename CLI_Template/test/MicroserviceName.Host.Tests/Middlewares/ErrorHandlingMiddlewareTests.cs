using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using MicroserviceName.Domain.Exceptions;
using MicroserviceName.Host.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Severity = MicroserviceName.Domain.Exceptions.Severity;

namespace MicroserviceName.Host.Tests.Middlewares
{
    public class ErrorHandlingMiddlewareTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly RequestDelegate _next = Mock.Of<RequestDelegate>();
        private readonly ErrorHandlingMiddleware _sut;

        public ErrorHandlingMiddlewareTests()
        {
            _sut = new ErrorHandlingMiddleware(_next);
        }

        [Fact]
        public async Task Invoke_ShouldPassContextToNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            Mock.Get(_next).Verify(next => next(context));
        }

        [Fact]
        public async Task Invoke_GivenExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHrrpContext();
            Mock.Get(_next)
                .Setup(d => d(context))
                .Throws<Exception>();

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            GetResponse(context)
                .Errors
                .First()
                .Should()
                .BeEquivalentTo(new Error(Severity.Unexpected, "Exception", "Exception of type 'System.Exception' was thrown."));
        }

        [Fact]
        public async Task Invoke_GivenValidationExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHrrpContext();
            var propertyName = _fixture.Create<string>();
            var errorMessage = _fixture.Create<string>();
            var errorCode = _fixture.Create<string>();
            Mock.Get(_next)
                .Setup(d => d(context))
                .Throws(new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(propertyName, errorMessage)
                    {
                        ErrorCode = errorCode
                    }
                }));

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            GetResponse(context)
                .Errors
                .First()
                .Should()
                .BeEquivalentTo(new Error(Severity.Correctable, errorCode, errorMessage));
        }

        [Fact]
        public async Task Invoke_GivenServiceClientExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHrrpContext();
            var expectedMessage = _fixture.Create<string>();
            var expectedStatusCode = _fixture.Create<HttpStatusCode>();
            Mock.Get(_next)
                .Setup(d => d(context))
                .Throws(new ServiceClientException(expectedMessage, expectedStatusCode));

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)expectedStatusCode);
            GetResponse(context)
                .Errors
                .First()
                .Should()
                .BeEquivalentTo(new Error(Severity.Unexpected, nameof(ServiceClientException), expectedMessage));
        }

        [Fact]
        public async Task Invoke_GivenAggregateExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHrrpContext();
            var expectedStatusCode = HttpStatusCode.InternalServerError;
            var expectedErrors = new Exception[]
            {
                new Exception("Error 1"),
                new Exception("Error 2")
            };
            Mock.Get(_next)
                .Setup(d => d(It.IsAny<HttpContext>()))
                .Throws(new AggregateException(expectedErrors));

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)expectedStatusCode);
            GetResponse(context)
                .Errors
                .Should()
                .Contain((error) => expectedErrors.Any(e => e.Message == error.Detail));
        }

        private HttpContext GetHrrpContext()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            return context;
        }

        private ErrorResponse GetResponse(HttpContext context)
        {
            var responseBody = context.Response.Body;
            responseBody.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(responseBody).ReadToEnd();
            return JsonConvert.DeserializeObject<ErrorResponse>(response);
        }

        private class ErrorResponse
        {
            public IEnumerable<Error> Errors { get; set; } = null!;
        }
    }
}
