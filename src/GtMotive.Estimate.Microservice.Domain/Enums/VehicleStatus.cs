namespace GtMotive.Estimate.Microservice.Domain.Enums
{
    /// <summary>
    /// Represents the status of a vehicle in the rental fleet.
    /// </summary>
    public enum VehicleStatus
    {
        /// <summary>
        /// Vehicle is available for rental.
        /// </summary>
        Available = 0,

        /// <summary>
        /// Vehicle is currently rented to a customer.
        /// </summary>
        Rented = 1,

        /// <summary>
        /// Vehicle is under maintenance and not available.
        /// </summary>
        Maintenance = 2,

        /// <summary>
        /// Vehicle is retired from the fleet.
        /// </summary>
        Retired = 3
    }
}
