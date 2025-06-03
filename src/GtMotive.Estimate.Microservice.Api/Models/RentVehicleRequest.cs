#nullable enable

using System.ComponentModel.DataAnnotations;

namespace GtMotive.Estimate.Microservice.Api.Models
{
    /// <summary>
    /// Request model for renting a vehicle.
    /// </summary>
    public class RentVehicleRequest
    {
        /// <summary>
        /// Gets or sets the vehicle identifier to rent.
        /// </summary>
        [Required]
        public string VehicleId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        [Required]
        public string CustomerId { get; set; } = string.Empty;
    }
}
