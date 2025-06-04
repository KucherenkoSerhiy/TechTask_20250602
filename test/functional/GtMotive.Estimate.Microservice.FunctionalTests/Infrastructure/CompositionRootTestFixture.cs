using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api;
using GtMotive.Estimate.Microservice.Infrastructure;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[assembly: CLSCompliant(false)]

namespace GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure
{
    public sealed class CompositionRootTestFixture : IDisposable, IAsyncLifetime
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly TestServer _testServer;
        private readonly HttpClient _httpClient;

        public CompositionRootTestFixture()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Set up TestServer for HTTP testing
            var webHostBuilder = new WebHostBuilder()
                .UseEnvironment("Test")
                .ConfigureServices(services =>
                {
                    services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
                    services.AddRouting();

                    // Add controllers with explicit assembly reference
                    services.AddControllers()
                        .AddApplicationPart(typeof(ApiConfiguration).Assembly);

                    // Add test authentication
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                            "Test", options => { });

                    services.AddAuthorization();

                    services.AddApiDependencies();
                    services.AddLogging();
                    services.AddBaseInfrastructure(true);
                    services.AddSingleton<IConfiguration>(configuration);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            _testServer = new TestServer(webHostBuilder);
            _httpClient = _testServer.CreateClient();

            // Also keep the service provider for direct service access when needed
            var services = new ServiceCollection();
            Configuration = configuration;
            ConfigureServices(services);
            services.AddSingleton<IConfiguration>(configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Creates an HTTP client for making API calls in tests.
        /// </summary>
        /// <returns>http client.</returns>
        public HttpClient CreateClient() => _httpClient;

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task UsingHandlerForRequest<TRequest>(Func<IRequestHandler<TRequest, Unit>, Task> handlerAction)
            where TRequest : IRequest
        {
            if (handlerAction == null)
            {
                throw new ArgumentNullException(nameof(handlerAction));
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, Unit>>();

            await handlerAction.Invoke(handler);
        }

        public async Task UsingHandlerForRequestResponse<TRequest, TResponse>(Func<IRequestHandler<TRequest, TResponse>, Task> handlerAction)
            where TRequest : IRequest<TResponse>
        {
            if (handlerAction == null)
            {
                throw new ArgumentNullException(nameof(handlerAction));
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            Debug.Assert(handler != null, "The requested handler has not been registered");

            await handlerAction.Invoke(handler);
        }

        public async Task UsingRepository<TRepository>(Func<TRepository, Task> handlerAction)
        {
            if (handlerAction == null)
            {
                throw new ArgumentNullException(nameof(handlerAction));
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TRepository>();

            Debug.Assert(handler != null, "The requested handler has not been registered");

            await handlerAction.Invoke(handler);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _testServer?.Dispose();
            _serviceProvider?.Dispose();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDb"));
            services.AddRouting();
            services.AddControllers();
            services.AddApiDependencies();
            services.AddLogging();
            services.AddBaseInfrastructure(true);
        }
    }
}
