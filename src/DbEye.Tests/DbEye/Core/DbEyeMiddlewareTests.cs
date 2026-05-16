using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DbEye.Common.Options;
using DbEye.Core.Collector;
using DbEye.Core.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentAssertions;
using DbEye.Common.Models;

namespace DbEye.Tests.DbEye.Core
{
    public class DbEyeMiddlewareTests
    {
        private readonly ILogger<DbEyeMiddleware> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly DbEyeCollector _collector;

        public DbEyeMiddlewareTests()
        {
            _logger = Substitute.For<ILogger<DbEyeMiddleware>>();
            _env = Substitute.For<IWebHostEnvironment>();
            _collector = new DbEyeCollector();
        }

        private HttpContext CreateHttpContext(string path = "/test", string method = "GET")
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Method = method;

            var services = new ServiceCollection();
            services.AddSingleton(_collector);
            context.RequestServices = services.BuildServiceProvider();

            return context;
        }

        private DbEyeMiddleware CreateMiddleware()
            => new DbEyeMiddleware(_ => Task.CompletedTask, _logger, _env);

        [Fact]
        [Trait("Core", "Middleware")]
        public async Task ShouldSkipMiddleware_WhenNotDevelopmentEnvironment()
        {
            _env.EnvironmentName.Returns("Production");

            var middleware = CreateMiddleware();
            var context = CreateHttpContext();

            await middleware.InvokeAsync(context, Options.Create(new DbEyeOptions()));

            _collector.Queries.Should().BeEmpty();
        }

        [Fact]
        [Trait("Core", "Middleware")]
        public async Task ShouldSkipMiddleware_WhenEndpointIsExcluded()
        {
            _env.EnvironmentName.Returns("Development");

            var options = new DbEyeOptions();
            options.ExcludeEndpoints("/test");

            var middleware = CreateMiddleware();
            var context = CreateHttpContext(path: "/test");

            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(10)));
            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(10)));

            await middleware.InvokeAsync(context, Options.Create(options));

            _logger.DidNotReceiveWithAnyArgs().Log(default, default, default!, default, default!);
        }

        [Fact]
        [Trait("Core", "Middleware")]
        public async Task ShouldLogWarning_WhenNPlusOneDetected()
        {
            _env.EnvironmentName.Returns("Development");

            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(10)));
            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(10)));

            var middleware = CreateMiddleware();
            var context = CreateHttpContext();

            await middleware.InvokeAsync(context, Options.Create(new DbEyeOptions()));

            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("N+1")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        [Trait("Core", "Middleware")]
        public async Task ShouldLogWarning_WhenSlowQueryDetected()
        {
            _env.EnvironmentName.Returns("Development");

            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(1000)));

            var middleware = CreateMiddleware();
            var context = CreateHttpContext();

            await middleware.InvokeAsync(context, Options.Create(new DbEyeOptions { SlowQueryThresholdMs = 500 }));

            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("Slow query")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        [Trait("Core", "Middleware")]
        public async Task ShouldLogInformation_WhenNoIssuesDetected()
        {
            _env.EnvironmentName.Returns("Development");

            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(10)));

            var middleware = CreateMiddleware();
            var context = CreateHttpContext();

            await middleware.InvokeAsync(context, Options.Create(new DbEyeOptions { SlowQueryThresholdMs = 500 }));

            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("no issues detected")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        [Trait("Core", "Middleware")]
        public async Task ShouldUseEndpointThreshold_WhenConfiguredForSpecificPath()
        {
            _env.EnvironmentName.Returns("Development");

            _collector.Add(new QueryModel("SELECT 1", TimeSpan.FromMilliseconds(200)));

            var options = new DbEyeOptions
            {
                SlowQueryThresholdMs = 500,
                EndpointThresholds = new Dictionary<string, int> { ["/test"] = 100 }
            };

            var middleware = CreateMiddleware();
            var context = CreateHttpContext(path: "/test");

            await middleware.InvokeAsync(context, Options.Create(options));

            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("Slow query")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}