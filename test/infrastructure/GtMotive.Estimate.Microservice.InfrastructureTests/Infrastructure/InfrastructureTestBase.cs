#nullable enable

using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Infrastructure
{
    [Collection(TestCollections.TestServer)]
    public abstract class InfrastructureTestBase : IAsyncLifetime
    {
        protected InfrastructureTestBase(GenericInfrastructureTestServerFixture fixture)
        {
            Fixture = fixture;

            // Use a unique database name per test to ensure isolation
            TestDatabaseName = $"VehicleRentalDB_Test_{Guid.NewGuid():N}";
        }

        protected GenericInfrastructureTestServerFixture Fixture { get; }

        protected string TestDatabaseName { get; }

        private MongoService? MongoService { get; set; }

        public virtual Task InitializeAsync()
        {
            // Initialize test database if needed
            return Task.CompletedTask;
        }

        public virtual async Task DisposeAsync()
        {
            // Clean up test database after each test
            if (MongoService != null)
            {
                try
                {
                    var client = new MongoClient("mongodb://localhost:27017");
                    await client.DropDatabaseAsync(TestDatabaseName);
                }
                catch (MongoException)
                {
                    // Ignore MongoDB-specific cleanup errors to prevent test failures
                }
                catch (TimeoutException)
                {
                    // Ignore timeout errors during cleanup
                }
            }
        }

        protected MongoService GetMongoService()
        {
            if (MongoService == null)
            {
                var mongoSettings = new MongoDbSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    MongoDbDatabaseName = TestDatabaseName
                };

                var optionsWrapper = Options.Create(mongoSettings);
                MongoService = new MongoService(optionsWrapper);
            }

            return MongoService;
        }
    }
}
