#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;

namespace GtMotive.Estimate.Microservice.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Vehicle aggregate.
    /// This is a port in hexagonal architecture - domain defines the contract.
    /// </summary>
    public interface IVehicleRepository
    {
        /// <summary>
        /// Gets a vehicle by its identifier.
        /// </summary>
        /// <param name="vehicleId">The vehicle identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The vehicle if found, null otherwise.</returns>
        Task<Vehicle?> GetByIdAsync(VehicleId vehicleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a vehicle by its license plate.
        /// </summary>
        /// <param name="licensePlate">The license plate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The vehicle if found, null otherwise.</returns>
        Task<Vehicle?> GetByLicensePlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the vehicle currently rented by a customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rented vehicle if found, null otherwise.</returns>
        Task<Vehicle?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all available vehicles in the fleet.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of available vehicles.</returns>
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new vehicle to the fleet.
        /// </summary>
        /// <param name="vehicle">The vehicle to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Empty.</returns>
        Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Empty.</returns>
        Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a license plate already exists in the fleet.
        /// </summary>
        /// <param name="licensePlate">The license plate to check.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ExistsWithLicensePlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken = default);
    }
}
