#nullable enable

using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Enums;
using GtMotive.Estimate.Microservice.Domain.Repositories;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using GtMotive.Estimate.Microservice.Infrastructure.Repositories;
using GtMotive.Estimate.Microservice.InfrastructureTests.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Repositories
{
    /// <summary>
    /// Integration tests for MongoVehicleRepository.
    /// Tests the repository with a real MongoDB connection.
    /// </summary>
    [Collection(TestCollections.TestServer)]
    public class MongoVehicleRepositoryShould : InfrastructureTestBase
    {
        private readonly IVehicleRepository _repository;

        public MongoVehicleRepositoryShould(GenericInfrastructureTestServerFixture fixture)
            : base(fixture)
        {
            // Use the base class's isolated MongoDB service
            var mongoService = GetMongoService();
            var mongoSettings = new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                MongoDbDatabaseName = TestDatabaseName
            };
            var optionsWrapper = Options.Create(mongoSettings);
            _repository = new MongoVehicleRepository(mongoService, optionsWrapper);
        }

        [Fact]
        public async Task PersistVehicle()
        {
            // Given
            var vehicle = CreateTestVehicle();

            // When
            await _repository.AddAsync(vehicle);

            // Then
            var retrieved = await _repository.GetByIdAsync(vehicle.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(vehicle.Id, retrieved!.Id);
            Assert.Equal(vehicle.LicensePlate.Value, retrieved.LicensePlate.Value);
        }

        [Fact]
        public async Task ReturnGetByLicensePlateVehicle()
        {
            // Given
            var licensePlate = new LicensePlate("TEST123");
            var vehicle = CreateTestVehicle(licensePlate: licensePlate);
            await _repository.AddAsync(vehicle);

            // When
            var retrieved = await _repository.GetByLicensePlateAsync(licensePlate);

            // Then
            Assert.NotNull(retrieved);
            Assert.Equal(vehicle.Id, retrieved!.Id);
        }

        [Fact]
        public async Task PersistUpdate()
        {
            // Given
            var vehicle = CreateTestVehicle();
            await _repository.AddAsync(vehicle);

            vehicle.Rent("customer123");

            // When
            await _repository.UpdateAsync(vehicle);

            // Then
            var retrieved = await _repository.GetByIdAsync(vehicle.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(VehicleStatus.Rented, retrieved!.Status);
            Assert.Equal("customer123", retrieved.CurrentCustomerId);
        }

        [Fact]
        public async Task ReturnTrueWhenVehicleExists()
        {
            // Given
            var licensePlate = new LicensePlate("EXISTS123");
            var vehicle = CreateTestVehicle(licensePlate: licensePlate);
            await _repository.AddAsync(vehicle);

            // When
            var exists = await _repository.ExistsWithLicensePlateAsync(licensePlate);

            // Then
            Assert.True(exists);
        }

        [Fact]
        public async Task ReturnFalseWhenVehicleDoesNotExist()
        {
            // Given
            var licensePlate = new LicensePlate("NOTFOUND");

            // When
            var exists = await _repository.ExistsWithLicensePlateAsync(licensePlate);

            // Then
            Assert.False(exists);
        }

        private static Vehicle CreateTestVehicle(VehicleId? vehicleId = null, LicensePlate? licensePlate = null)
        {
            return new Vehicle(
                vehicleId ?? VehicleId.New(),
                licensePlate ?? new LicensePlate($"TEST{Guid.NewGuid().ToString()[..6]}"),
                new ManufacturingDate(DateTime.Now.AddYears(-2)),
                "Test Model",
                "Test Brand");
        }
    }
}
