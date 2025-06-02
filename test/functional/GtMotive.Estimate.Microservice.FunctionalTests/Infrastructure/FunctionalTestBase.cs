using System.Threading.Tasks;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure
{
    [Collection(TestCollections.Functional)]
    public abstract class FunctionalTestBase : IAsyncLifetime
    {
        public const int QueueWaitingTimeInMilliseconds = 1000;

        protected FunctionalTestBase(CompositionRootTestFixture fixture)
        {
            Fixture = fixture;
        }

        protected CompositionRootTestFixture Fixture { get; }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }
}
