#nullable enable

using System;
using GtMotive.Estimate.Microservice.Domain.Enums;

namespace GtMotive.Estimate.Microservice.Api.Models
{
    /// <summary>
    /// Vehicle data transfer object for API responses.
    /// </summary>
    public class VehicleDto
    {
        /// <summary>
        /// Gets or sets the vehicle identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the license plate.
        /// </summary>
        public string LicensePlate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the manufacturing date.
        /// </summary>
        public DateTime ManufacturingDate { get; set; }

        /// <summary>
        /// Gets or sets the vehicle model.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle brand.
        /// </summary>
        public string Brand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the vehicle.
        /// </summary>
        public VehicleStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the current customer ID if the vehicle is rented.
        /// </summary>
        public string? CurrentCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the date when the vehicle was rented.
        /// </summary>
        public DateTime? RentedAt { get; set; }
    }
}
