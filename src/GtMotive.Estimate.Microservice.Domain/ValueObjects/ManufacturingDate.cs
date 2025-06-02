using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GtMotive.Estimate.Microservice.Domain.ValueObjects
{
    /// <summary>
    /// Manufacturing date value object.
    /// Represents a vehicle manufacturing date with business rule validation (maximum 5 years old).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed record ManufacturingDate
    {
        private const int MaxAgeInYears = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManufacturingDate"/> class.
        /// </summary>
        /// <param name="value">The manufacturing date value.</param>
        /// <exception cref="DomainException">Thrown when the date violates business rules.</exception>
        public ManufacturingDate(DateTime value)
        {
            ValidateManufacturingDate(value);
            Value = value.Date; // Normalize to date only
        }

        /// <summary>
        /// Gets the manufacturing date value.
        /// </summary>
        public DateTime Value { get; }

        /// <summary>
        /// Determines if this manufacturing date is valid for fleet inclusion.
        /// </summary>
        /// <returns>true if the date is valid for fleet; otherwise, false.</returns>
        public bool IsValidForFleet()
        {
            try
            {
                ValidateManufacturingDate(Value);
                return true;
            }
            catch (DomainException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a string representation of the ManufacturingDate.
        /// </summary>
        /// <returns>The manufacturing date in yyyy-MM-dd format.</returns>
        public override string ToString()
        {
            return Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Validates the manufacturing date according to business rules.
        /// </summary>
        /// <param name="value">The date to validate.</param>
        /// <exception cref="DomainException">Thrown when the date is invalid.</exception>
        private static void ValidateManufacturingDate(DateTime value)
        {
            var now = DateTime.Now;

            if (value.Date > now.Date)
            {
                throw new DomainException("Vehicle manufacturing date cannot be in the future");
            }

            var maxAllowedDate = now.AddYears(-MaxAgeInYears).Date;
            if (value.Date < maxAllowedDate)
            {
                throw new DomainException("Vehicle manufacturing date cannot be older than 5 years");
            }
        }
    }
}
