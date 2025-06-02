using System.Globalization;
using System.Text.RegularExpressions;

namespace GtMotive.Estimate.Microservice.Domain.ValueObjects
{
    /// <summary>
    /// License plate value object.
    /// Represents a vehicle license plate with validation rules.
    /// </summary>
    public sealed record LicensePlate
    {
        private const int MinLength = 3;
        private const int MaxLength = 10;
        private static readonly Regex LicensePlatePattern = new("^[A-Z0-9]+$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensePlate"/> class.
        /// </summary>
        /// <param name="value">The license plate value.</param>
        /// <exception cref="DomainException">Thrown when the value is invalid.</exception>
        public LicensePlate(string value)
        {
            ValidateLicensePlate(value);
            Value = value.ToUpperInvariant();
        }

        /// <summary>
        /// Gets the license plate value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Returns a string representation of the LicensePlate.
        /// </summary>
        /// <returns>The license plate value.</returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Validates the license plate value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <exception cref="DomainException">Thrown when the value is invalid.</exception>
        private static void ValidateLicensePlate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("License plate cannot be null or empty");
            }

            if (value.Length is < MinLength or > MaxLength)
            {
                throw new DomainException("License plate must be between 3 and 10 characters");
            }

            if (!LicensePlatePattern.IsMatch(value.ToUpper(CultureInfo.InvariantCulture)))
            {
                throw new DomainException("License plate can only contain letters and numbers");
            }
        }
    }
}
