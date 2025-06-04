#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Mapping;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.ApplicationCore.Services;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Controllers
{
    /// <summary>
    /// Controller for vehicle rental operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleController"/> class.
        /// </summary>
        /// <param name="rentalService">The rental service.</param>
        public VehicleController(IRentalService rentalService)
        {
            _rentalService = rentalService ?? throw new ArgumentNullException(nameof(rentalService));
        }

        /// <summary>
        /// Gets all available vehicles for rental.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of available vehicles.</returns>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAvailableVehicles(
            CancellationToken cancellationToken = default)
        {
            var vehicles = await _rentalService.GetAvailableVehiclesAsync(cancellationToken);
            var vehicleDtos = VehicleMapper.ToDto(vehicles);
            return Ok(vehicleDtos);
        }

        /// <summary>
        /// Gets the vehicle currently rented by a customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rented vehicle if found.</returns>
        [HttpGet("customer/{customerId}/rental")]
        [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VehicleDto?>> GetCustomerRentedVehicle(string customerId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return BadRequest("Customer ID cannot be null or empty");
            }

            var vehicle = await _rentalService.GetCustomerRentedVehicleAsync(customerId, cancellationToken);

            if (vehicle == null)
            {
                return NotFound($"No active rental found for customer {customerId}");
            }

            var vehicleDto = VehicleMapper.ToDto(vehicle);
            return Ok(vehicleDto);
        }

        /// <summary>
        /// Rents a vehicle to a customer.
        /// </summary>
        /// <param name="request">The rent vehicle request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The rented vehicle.</returns>
        [HttpPost("rent")]
        [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VehicleDto>> RentVehicle([FromBody][NotNull] RentVehicleRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Guid.TryParse(request.VehicleId, out var vehicleGuid))
            {
                return BadRequest("Invalid vehicle ID format");
            }

            try
            {
                var vehicleId = new VehicleId(vehicleGuid);
                var vehicle = await _rentalService.RentVehicleAsync(vehicleId, request.CustomerId, cancellationToken);
                var vehicleDto = VehicleMapper.ToDto(vehicle);

                return Ok(vehicleDto);
            }
            catch (DomainException ex) when (ex.Message.Contains("Vehicle not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ex.Message);
            }
            catch (DomainException ex) when (ex.Message.Contains("already has an active rental", StringComparison.OrdinalIgnoreCase) ||
                                           ex.Message.Contains("not available for rental", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(ex.Message);
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Returns a rented vehicle.
        /// </summary>
        /// <param name="request">The return vehicle request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The returned vehicle.</returns>
        [HttpPost("return")]
        [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VehicleDto>> ReturnVehicle([FromBody][NotNull] ReturnVehicleRequest request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Guid.TryParse(request.VehicleId, out var vehicleGuid))
            {
                return BadRequest("Invalid vehicle ID format");
            }

            try
            {
                var vehicleId = new VehicleId(vehicleGuid);
                var vehicle = await _rentalService.ReturnVehicleAsync(vehicleId, request.CustomerId, cancellationToken);
                var vehicleDto = VehicleMapper.ToDto(vehicle);

                return Ok(vehicleDto);
            }
            catch (DomainException ex) when (ex.Message.Contains("Vehicle not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ex.Message);
            }
            catch (DomainException ex) when (ex.Message.Contains("not rented by this customer", StringComparison.OrdinalIgnoreCase) ||
                                           ex.Message.Contains("not currently rented", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(ex.Message);
            }
            catch (DomainException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
