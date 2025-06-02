#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;

namespace GtMotive.Estimate.Microservice.ApplicationCore.Services
{
    /// <summary>
    /// Application service for vehicle rental operations.
    /// Orchestrates business logic and enforces rental rules.
    /// </summary>
    public interface IRentalService
    {
        /// <summary>
        /// Rents a vehicle to a customer.
        /// Enforces business rule: maximum 1 vehicle per customer.
        /// </summary>
        /// <param name="vehicleId">The vehicle to rent.</param>
        /// <param name="customerId">The customer renting the vehicle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rented vehicle.</returns>
        Task<Vehicle> RentVehicleAsync(VehicleId vehicleId, string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a rented vehicle.
        /// </summary>
        /// <param name="vehicleId">The vehicle to return.</param>
        /// <param name="customerId">The customer returning the vehicle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The returned vehicle.</returns>
        Task<Vehicle> ReturnVehicleAsync(VehicleId vehicleId, string customerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all available vehicles for rental.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of available vehicles.</returns>
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the vehicle currently rented by a customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rented vehicle if found, null otherwise.</returns>
        Task<Vehicle?> GetCustomerRentedVehicleAsync(string customerId, CancellationToken cancellationToken = default);
    }
}
