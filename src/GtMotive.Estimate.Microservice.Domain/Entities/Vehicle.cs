#nullable enable

using GtMotive.Estimate.Microservice.Domain.Enums;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using System;

namespace GtMotive.Estimate.Microservice.Domain.Entities
{
    /// <summary>
    /// Vehicle aggregate root.
    /// Represents a vehicle in the rental fleet with business rules for rental operations.
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vehicle"/> class.
        /// </summary>
        /// <param name="id">The vehicle identifier.</param>
        /// <param name="licensePlate">The license plate.</param>
        /// <param name="manufacturingDate">The manufacturing date.</param>
        /// <param name="model">The vehicle model.</param>
        /// <param name="brand">The vehicle brand.</param>
        /// <exception cref="DomainException">Thrown when validation fails.</exception>
        public Vehicle(VehicleId id, LicensePlate licensePlate, ManufacturingDate manufacturingDate, string model, string brand)
        {
            ValidateModel(model);
            ValidateBrand(brand);

            Id = id ?? throw new ArgumentNullException(nameof(id));
            LicensePlate = licensePlate ?? throw new ArgumentNullException(nameof(licensePlate));
            ManufacturingDate = manufacturingDate ?? throw new ArgumentNullException(nameof(manufacturingDate));
            Model = model;
            Brand = brand;
            Status = VehicleStatus.Available;
        }

        /// <summary>
        /// Gets the vehicle identifier.
        /// </summary>
        public VehicleId Id { get; }

        /// <summary>
        /// Gets the license plate.
        /// </summary>
        public LicensePlate LicensePlate { get; }

        /// <summary>
        /// Gets the manufacturing date.
        /// </summary>
        public ManufacturingDate ManufacturingDate { get; }

        /// <summary>
        /// Gets the vehicle model.
        /// </summary>
        public string Model { get; }

        /// <summary>
        /// Gets the vehicle brand.
        /// </summary>
        public string Brand { get; }

        /// <summary>
        /// Gets the current status of the vehicle.
        /// </summary>
        public VehicleStatus Status { get; private set; }

        /// <summary>
        /// Gets the current customer ID if the vehicle is rented.
        /// </summary>
        public string? CurrentCustomerId { get; private set; }

        /// <summary>
        /// Gets the date when the vehicle was rented.
        /// </summary>
        public DateTime? RentedAt { get; private set; }

        /// <summary>
        /// Rents the vehicle to a customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <exception cref="DomainException">Thrown when the vehicle is not available for rental.</exception>
        public void Rent(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
            {
                throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));
            }

            if (!IsAvailableForRental())
            {
                throw new DomainException("Vehicle is not available for rental");
            }

            Status = VehicleStatus.Rented;
            CurrentCustomerId = customerId;
            RentedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns the vehicle from rental.
        /// </summary>
        /// <exception cref="DomainException">Thrown when the vehicle is not currently rented.</exception>
        public void Return()
        {
            if (Status != VehicleStatus.Rented)
            {
                throw new DomainException("Vehicle is not currently rented");
            }

            Status = VehicleStatus.Available;
            CurrentCustomerId = null;
            RentedAt = null;
        }

        /// <summary>
        /// Sets the vehicle status.
        /// </summary>
        /// <param name="status">The new status.</param>
        public void SetStatus(VehicleStatus status)
        {
            Status = status;

            // Clear rental information if not rented
            if (status != VehicleStatus.Rented)
            {
                CurrentCustomerId = null;
                RentedAt = null;
            }
        }

        /// <summary>
        /// Determines if the vehicle is available for rental.
        /// </summary>
        /// <returns>True if available for rental, false otherwise.</returns>
        public bool IsAvailableForRental()
        {
            return Status == VehicleStatus.Available;
        }

        /// <summary>
        /// Validates the vehicle model.
        /// </summary>
        /// <param name="model">The model to validate.</param>
        /// <exception cref="DomainException">Thrown when the model is invalid.</exception>
        private static void ValidateModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new DomainException("Vehicle model cannot be null or empty");
            }
        }

        /// <summary>
        /// Validates the vehicle brand.
        /// </summary>
        /// <param name="brand">The brand to validate.</param>
        /// <exception cref="DomainException">Thrown when the brand is invalid.</exception>
        private static void ValidateBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new DomainException("Vehicle brand cannot be null or empty");
            }
        }
    }
}
