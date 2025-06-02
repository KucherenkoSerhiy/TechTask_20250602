using System;
using System.Linq;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.Services;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Enums;
using GtMotive.Estimate.Microservice.Domain.Repositories;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.ApplicationCore.Services
{
    /// <summary>
    /// Tests for RentalService application service.
    /// BDD Scenario: Vehicle rental business logic and rules enforcement.
    /// </summary>
    public class RentalServiceShould
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly IRentalService _rentalService;

        public RentalServiceShould()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _rentalService = new RentalService(_vehicleRepositoryMock.Object);
        }

        [Fact]
        public async Task RentVehicleSuccessfullyWhenVehicleIsAvailableAndCustomerHasNoActiveRental()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";
            var availableVehicle = CreateTestVehicle(vehicleId);

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync(availableVehicle);
            _vehicleRepositoryMock.Setup(x => x.GetByCustomerIdAsync(customerId, default))
                .ReturnsAsync((Vehicle?)null);

            // When (Act)
            var result = await _rentalService.RentVehicleAsync(vehicleId, customerId);

            // Then (Assert)
            Assert.NotNull(result);
            Assert.Equal(VehicleStatus.Rented, result.Status);
            Assert.Equal(customerId, result.CurrentCustomerId);
            _vehicleRepositoryMock.Verify(x => x.UpdateAsync(availableVehicle, default), Times.Once);
        }

        [Fact]
        public async Task ThrowDomainExceptionWhenTryingToRentNonExistentVehicle()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync((Vehicle?)null);

            // When & Then (Act & Assert)
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _rentalService.RentVehicleAsync(vehicleId, customerId));
            Assert.Equal("Vehicle not found", exception.Message);
        }

        [Fact]
        public async Task ThrowDomainExceptionWhenCustomerAlreadyHasActiveRental()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";
            var availableVehicle = CreateTestVehicle(vehicleId);
            var customerCurrentVehicle = CreateTestVehicle(VehicleId.New());
            customerCurrentVehicle.Rent(customerId);

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync(availableVehicle);
            _vehicleRepositoryMock.Setup(x => x.GetByCustomerIdAsync(customerId, default))
                .ReturnsAsync(customerCurrentVehicle);

            // When & Then (Act & Assert)
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _rentalService.RentVehicleAsync(vehicleId, customerId));
            Assert.Equal("Customer already has an active rental", exception.Message);
        }

        [Fact]
        public async Task ThrowDomainExceptionWhenTryingToRentUnavailableVehicle()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";
            var rentedVehicle = CreateTestVehicle(vehicleId);
            rentedVehicle.Rent("another-customer");

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync(rentedVehicle);
            _vehicleRepositoryMock.Setup(x => x.GetByCustomerIdAsync(customerId, default))
                .ReturnsAsync((Vehicle?)null);

            // When & Then (Act & Assert)
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _rentalService.RentVehicleAsync(vehicleId, customerId));
            Assert.Equal("Vehicle is not available for rental", exception.Message);
        }

        [Fact]
        public async Task ReturnVehicleSuccessfullyWhenCustomerIsCurrentRenter()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";
            var rentedVehicle = CreateTestVehicle(vehicleId);
            rentedVehicle.Rent(customerId);

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync(rentedVehicle);

            // When (Act)
            var result = await _rentalService.ReturnVehicleAsync(vehicleId, customerId);

            // Then (Assert)
            Assert.NotNull(result);
            Assert.Equal(VehicleStatus.Available, result.Status);
            Assert.Null(result.CurrentCustomerId);
            _vehicleRepositoryMock.Verify(x => x.UpdateAsync(rentedVehicle, default), Times.Once);
        }

        [Fact]
        public async Task ThrowDomainExceptionWhenTryingToReturnNonExistentVehicle()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync((Vehicle?)null);

            // When & Then (Act & Assert)
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _rentalService.ReturnVehicleAsync(vehicleId, customerId));
            Assert.Equal("Vehicle not found", exception.Message);
        }

        [Fact]
        public async Task ThrowDomainExceptionWhenTryingToReturnVehicleRentedByDifferentCustomer()
        {
            // Given (Arrange)
            var vehicleId = VehicleId.New();
            var customerId = "customer123";
            var differentCustomerId = "different-customer";
            var rentedVehicle = CreateTestVehicle(vehicleId);
            rentedVehicle.Rent(differentCustomerId);

            _vehicleRepositoryMock.Setup(x => x.GetByIdAsync(vehicleId, default))
                .ReturnsAsync(rentedVehicle);

            // When & Then (Act & Assert)
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _rentalService.ReturnVehicleAsync(vehicleId, customerId));
            Assert.Equal("Vehicle is not rented by this customer", exception.Message);
        }

        [Fact]
        public async Task GetAvailableVehiclesSuccessfully()
        {
            // Given (Arrange)
            var availableVehicles = new[] { CreateTestVehicle(VehicleId.New()), CreateTestVehicle(VehicleId.New()) };

            _vehicleRepositoryMock.Setup(x => x.GetAvailableVehiclesAsync(default))
                .ReturnsAsync(availableVehicles);

            // When (Act)
            var result = await _rentalService.GetAvailableVehiclesAsync();

            // Then (Assert)
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCustomerRentedVehicleSuccessfully()
        {
            // Given (Arrange)
            var customerId = "customer123";
            var rentedVehicle = CreateTestVehicle(VehicleId.New());
            rentedVehicle.Rent(customerId);

            _vehicleRepositoryMock.Setup(x => x.GetByCustomerIdAsync(customerId, default))
                .ReturnsAsync(rentedVehicle);

            // When (Act)
            var result = await _rentalService.GetCustomerRentedVehicleAsync(customerId);

            // Then (Assert)
            Assert.NotNull(result);
            Assert.Equal(customerId, result?.CurrentCustomerId);
            Assert.Equal(VehicleStatus.Rented, result?.Status);
        }

        [Fact]
        public async Task ReturnNullWhenCustomerHasNoActiveRental()
        {
            // Given (Arrange)
            var customerId = "customer123";

            _vehicleRepositoryMock.Setup(x => x.GetByCustomerIdAsync(customerId, default))
                .ReturnsAsync((Vehicle?)null);

            // When (Act)
            var result = await _rentalService.GetCustomerRentedVehicleAsync(customerId);

            // Then (Assert)
            Assert.Null(result);
        }

        private static Vehicle CreateTestVehicle(VehicleId vehicleId)
        {
            return new Vehicle(
                vehicleId,
                new LicensePlate("TEST123"),
                new ManufacturingDate(DateTime.Now.AddYears(-2)),
                "Test Model",
                "Test Brand");
        }
    }
}
