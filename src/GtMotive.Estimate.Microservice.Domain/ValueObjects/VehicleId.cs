using System;
using System.Diagnostics.CodeAnalysis;

namespace GtMotive.Estimate.Microservice.Domain.ValueObjects
{
    /// <summary>
    /// Vehicle identifier value object.
    /// Represents a strongly-typed vehicle ID following DDD patterns.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed record VehicleId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleId"/> class.
        /// </summary>
        /// <param name="value">The vehicle ID value.</param>
        /// <exception cref="DomainException">Thrown when the value is empty.</exception>
        public VehicleId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new DomainException("VehicleId cannot be empty");
            }

            Value = value;
        }

        /// <summary>
        /// Gets the vehicle ID value.
        /// </summary>
        public Guid Value { get; }

        /// <summary>
        /// Creates a new VehicleId with a new GUID.
        /// </summary>
        /// <returns>A new VehicleId instance.</returns>
        public static VehicleId New()
        {
            return new VehicleId(Guid.NewGuid());
        }

        /// <summary>
        /// Returns a string representation of the VehicleId.
        /// </summary>
        /// <returns>The string representation of the underlying GUID.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
