using System;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Enums;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.Entities
{
    /// <summary>
    /// Tests for Vehicle entity.
    /// BDD Scenario: Vehicle creation and rental business rules.
    /// </summary>
    public class VehicleShould
    {
        [Fact]
        public void CreateSuccessfullyWhenGivenValidData()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var licensePlate = new LicensePlate("ABC1234");
            var manufacturingDate = new ManufacturingDate(DateTime.Now.AddYears(-2));
            var model = "Toyota Camry";
            var brand = "Toyota";

            // When (Act)
            var vehicle = new Vehicle(vehicleId, licensePlate, manufacturingDate, model, brand);

            // Then (Assert)
            Assert.NotNull(vehicle);
            Assert.Equal(vehicleId, vehicle.Id);
            Assert.Equal(licensePlate, vehicle.LicensePlate);
            Assert.Equal(manufacturingDate, vehicle.ManufacturingDate);
            Assert.Equal(model, vehicle.Model);
            Assert.Equal(brand, vehicle.Brand);
            Assert.Equal(VehicleStatus.Available, vehicle.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowDomainExceptionWhenGivenInvalidModel(string invalidModel)
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var licensePlate = new LicensePlate("ABC1234");
            var manufacturingDate = new ManufacturingDate(DateTime.Now.AddYears(-2));
            var brand = "Toyota";

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() =>
                new Vehicle(vehicleId, licensePlate, manufacturingDate, invalidModel, brand));
            Assert.Equal("Vehicle model cannot be null or empty", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowDomainExceptionWhenGivenInvalidBrand(string invalidBrand)
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var licensePlate = new LicensePlate("ABC1234");
            var manufacturingDate = new ManufacturingDate(DateTime.Now.AddYears(-2));
            var model = "Camry";

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() =>
                new Vehicle(vehicleId, licensePlate, manufacturingDate, model, invalidBrand));
            Assert.Equal("Vehicle brand cannot be null or empty", exception.Message);
        }

        [Fact]
        public void RentSuccessfullyWhenVehicleIsAvailable()
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();
            var customerId = "customer123";

            // When (Act)
            vehicle.Rent(customerId);

            // Then (Assert)
            Assert.Equal(VehicleStatus.Rented, vehicle.Status);
            Assert.Equal(customerId, vehicle.CurrentCustomerId);
            Assert.True(vehicle.RentedAt.HasValue);
            Assert.True(vehicle.RentedAt.Value <= DateTime.UtcNow);
        }

        [Fact]
        public void ThrowDomainExceptionWhenTryingToRentAlreadyRentedVehicle()
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();
            vehicle.Rent("customer1");

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => vehicle.Rent("customer2"));
            Assert.Equal("Vehicle is not available for rental", exception.Message);
        }

        [Theory]
        [InlineData(VehicleStatus.Maintenance)]
        [InlineData(VehicleStatus.Retired)]
        public void ThrowDomainExceptionWhenTryingToRentUnavailableVehicle(VehicleStatus status)
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();
            vehicle.SetStatus(status);

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => vehicle.Rent("customer1"));
            Assert.Equal("Vehicle is not available for rental", exception.Message);
        }

        [Fact]
        public void ReturnSuccessfullyWhenVehicleIsRented()
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();
            vehicle.Rent("customer1");

            // When (Act)
            vehicle.Return();

            // Then (Assert)
            Assert.Equal(VehicleStatus.Available, vehicle.Status);
            Assert.Null(vehicle.CurrentCustomerId);
            Assert.Null(vehicle.RentedAt);
        }

        [Fact]
        public void ThrowDomainExceptionWhenTryingToReturnNonRentedVehicle()
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => vehicle.Return());
            Assert.Equal("Vehicle is not currently rented", exception.Message);
        }

        [Fact]
        public void BeAvailableForRentalWhenStatusIsAvailable()
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();

            // When (Act)
            var isAvailable = vehicle.IsAvailableForRental();

            // Then (Assert)
            Assert.True(isAvailable);
        }

        [Theory]
        [InlineData(VehicleStatus.Rented)]
        [InlineData(VehicleStatus.Maintenance)]
        [InlineData(VehicleStatus.Retired)]
        public void NotBeAvailableForRentalWhenStatusIsNotAvailable(VehicleStatus status)
        {
            // Given (Arrange)
            var vehicle = CreateTestVehicle();
            vehicle.SetStatus(status);

            // When (Act)
            var isAvailable = vehicle.IsAvailableForRental();

            // Then (Assert)
            Assert.False(isAvailable);
        }

        private static Vehicle CreateTestVehicle()
        {
            return new Vehicle(
                VehicleId.New(),
                new LicensePlate("TEST123"),
                new ManufacturingDate(DateTime.Now.AddYears(-2)),
                "Test Model",
                "Test Brand");
        }
    }
}
