using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MicroserviceName.Host.AppSettings;
using MicroserviceName.Host.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace MicroserviceName.Host.Tests.Middlewares
{
    public class DiagnosticsMiddlewareTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly AppMetadataSettings _appMetadata;
        private readonly RequestDelegate _next = Mock.Of<RequestDelegate>();
        private readonly DiagnosticsMiddleware _sut;

        public DiagnosticsMiddlewareTests()
        {
            _appMetadata = _fixture.Create<AppMetadataSettings>();
            _sut = new DiagnosticsMiddleware(_next, _appMetadata);
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
        public async Task Invoke_GivenRequestUriIsEqualToDiagnosticsPath_ShouldReturnAppMetadata()
        {
            // Arrange
            var context = GetHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            var responseBody = context.Response.Body;
            responseBody.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(responseBody).ReadToEnd();
            var appMetadata = JsonSerializer.Deserialize<AppMetadataSettings>(response);
            appMetadata.Should().NotBeNull().And.BeEquivalentTo(_appMetadata);
        }

        [Fact]
        public async Task Invoke_GivenRequestUriIsEqualToDiagnosticsPath_ShouldNotPassContextToNextDelegate()
        {
            // Arrange
            var context = GetHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            var mock = Mock.Get(_next);
            mock.Verify(next => next(context), Times.Never);
        }

        private HttpContext GetHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/version";
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            return context;
        }
    }
}
