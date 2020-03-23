using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MicroserviceName.Host.Middlewares;
using Xunit;

namespace MicroserviceName.Host.Tests.Middlewares
{
    public class ErrorLoggingMiddlewareTests
    {
        private readonly RequestDelegate _next = Mock.Of<RequestDelegate>();
        private readonly ILogger<ErrorLoggingMiddleware> _logger = Mock.Of<ILogger<ErrorLoggingMiddleware>>();
        private readonly ErrorLoggingMiddleware _sut;

        public ErrorLoggingMiddlewareTests()
        {
            _sut = new ErrorLoggingMiddleware(_next, _logger);
        }

        [Fact]
        public async Task Invoke_ShouldPassContextToNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            Mock.Get(_next).Verify(next => next(context), Times.Once);
        }

        [Fact]
        public async Task Invoke_GivenExceptionIsThrown_ShouldLogException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            Mock.Get(_next)
                .Setup(next => next(context))
                .Throws<Exception>();

            // Act
            await _sut.Invoke(context);

            // Assert
            // LogError, LogInfo and the rest are extension methods, so the only way to test the call is to this specific Log overload
            Mock.Get(_logger).Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
