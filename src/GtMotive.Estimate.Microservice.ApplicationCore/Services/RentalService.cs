#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Repositories;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Services
{
    /// <summary>
    /// Application service for vehicle rental operations.
    /// Orchestrates business logic and enforces rental rules.
    /// </summary>
    public class RentalService : IRentalService
    {
        private readonly IVehicleRepository _vehicleRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RentalService"/> class.
        /// </summary>
        /// <param name="vehicleRepository">The vehicle repository.</param>
        public RentalService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        }

        /// <inheritdoc />
        public async Task<Vehicle> RentVehicleAsync(VehicleId vehicleId, string customerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));
            }

            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle == null)
            {
                throw new DomainException("Vehicle not found");
            }

            // Enforce business rule: maximum 1 vehicle per customer
            var customerCurrentVehicle = await _vehicleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            if (customerCurrentVehicle != null)
            {
                throw new DomainException("Customer already has an active rental");
            }

            // Rent the vehicle (this will throw if vehicle is not available)
            vehicle.Rent(customerId);

            // Persist the changes
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

            return vehicle;
        }

        /// <inheritdoc />
        public async Task<Vehicle> ReturnVehicleAsync(VehicleId vehicleId, string customerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));
            }

            // Check if vehicle exists
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
            if (vehicle == null)
            {
                throw new DomainException("Vehicle not found");
            }

            // Verify the customer is the current renter
            if (vehicle.CurrentCustomerId != customerId)
            {
                throw new DomainException("Vehicle is not rented by this customer");
            }

            // Return the vehicle
            vehicle.Return();

            // Persist the changes
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

            return vehicle;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(CancellationToken cancellationToken = default)
        {
            return await _vehicleRepository.GetAvailableVehiclesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Vehicle?> GetCustomerRentedVehicleAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return string.IsNullOrWhiteSpace(customerId)
                ? throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId))
                : await _vehicleRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        }
    }
}
