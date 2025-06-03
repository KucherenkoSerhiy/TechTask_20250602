#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.Domain.Entities;

namespace GtMotive.Estimate.Microservice.Api.Mapping
{
    /// <summary>
    /// Mapper for converting between Vehicle entities and DTOs.
    /// </summary>
    public static class VehicleMapper
    {
        /// <summary>
        /// Converts a Vehicle domain entity to a VehicleDto.
        /// </summary>
        /// <param name="vehicle">The vehicle entity to convert.</param>
        /// <returns>The vehicle DTO.</returns>
        public static VehicleDto ToDto(Vehicle vehicle)
        {
            return vehicle == null
                ? throw new ArgumentNullException(nameof(vehicle))
                : new VehicleDto
                {
                    Id = vehicle.Id.Value.ToString(),
                    LicensePlate = vehicle.LicensePlate.Value,
                    ManufacturingDate = vehicle.ManufacturingDate.Value,
                    Model = vehicle.Model,
                    Brand = vehicle.Brand,
                    Status = vehicle.Status,
                    CurrentCustomerId = vehicle.CurrentCustomerId,
                    RentedAt = vehicle.RentedAt
                };
        }

        /// <summary>
        /// Converts a collection of Vehicle entities to DTOs.
        /// </summary>
        /// <param name="vehicles">The vehicles to convert.</param>
        /// <returns>Collection of vehicle DTOs.</returns>
        public static IEnumerable<VehicleDto> ToDto(IEnumerable<Vehicle> vehicles)
        {
            return vehicles == null ? throw new ArgumentNullException(nameof(vehicles)) : vehicles.Select(ToDto);
        }
    }
}
