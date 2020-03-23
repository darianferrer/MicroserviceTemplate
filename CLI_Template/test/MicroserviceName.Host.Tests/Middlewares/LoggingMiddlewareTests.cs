using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using MicroserviceName.Host.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MicroserviceName.Host.Tests.Middlewares
{
    public class LoggingMiddlewareTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly RequestDelegate _next = Mock.Of<RequestDelegate>();
        private readonly ILogger<LoggingMiddleware> _logger = Mock.Of<ILogger<LoggingMiddleware>>();
        private readonly LoggingMiddleware _sut;

        public LoggingMiddlewareTests()
        {
            _sut = new LoggingMiddleware(_next, _logger);
        }

        [Fact]
        public async Task Invoke_ShouldPassContextToNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            var mock = Mock.Get(_next);
            mock.Verify(next => next(context), Times.Once);
        }

        [Fact]
        public async Task Invoke_GivenConversationIdHeader_ShouldCreateLoggingScopeWithThatHeaderValue()
        {
            // Arrange
            var conversationId = _fixture.Create<string>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add("ConversationId", conversationId);

            // Act
            await _sut.Invoke(context);

            // Assert
            var mock = Mock.Get(_logger);
            var key = "conversationid";
            mock.Verify
            (
                logger => logger.BeginScope(It.Is<Dictionary<string, object>>(state => state.ContainsKey(key) && state[key].Equals(conversationId))),
                Times.Once
            );
        }

        [Fact]
        public async Task Invoke_GivenIpAddressInRequest_ShouldCreateLoggingScopeWithThatIpValue()
        {
            // Arrange
            var ip = 16777343; // 127.0.0.1
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = new System.Net.IPAddress(ip);

            // Act
            await _sut.Invoke(context);

            // Assert
            var mock = Mock.Get(_logger);
            var key = "clientip";
            mock.Verify
            (
                logger => logger.BeginScope(It.Is<Dictionary<string, object>>(state => state.ContainsKey(key) && state[key].Equals("127.0.0.1"))),
                Times.Once
            );
        }
    }
}
