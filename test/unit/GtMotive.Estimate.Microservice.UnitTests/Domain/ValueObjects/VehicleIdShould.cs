using System;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.ValueObjects
{
    /// <summary>
    /// Tests for VehicleId value object.
    /// BDD Scenario: Valid vehicle ID creation.
    /// </summary>
    public class VehicleIdShould
    {
        [Fact]
        public void CreateSuccessfullyWhenGivenValidGuid()
        {
            // Given (Arrange)
            var validGuid = Guid.NewGuid();

            // When (Act)
            var vehicleId = new VehicleId(validGuid);

            // Then (Assert)
            Assert.NotNull(vehicleId);
            Assert.Equal(validGuid, vehicleId.Value);
        }

        [Fact]
        public void ThrowDomainExceptionWhenGivenEmptyGuid()
        {
            // Given (Arrange)
            var emptyGuid = Guid.Empty;

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new VehicleId(emptyGuid));
            Assert.Equal("VehicleId cannot be empty", exception.Message);
        }

        [Fact]
        public void BeEqualWhenComparingTwoVehicleIdsWithSameValue()
        {
            // Given (Arrange)
            var guid = Guid.NewGuid();
            var vehicleId1 = new VehicleId(guid);
            var vehicleId2 = new VehicleId(guid);

            // When & Then (Act & Assert)
            Assert.Equal(vehicleId1, vehicleId2);
            Assert.True(vehicleId1 == vehicleId2);
            Assert.False(vehicleId1 != vehicleId2);
            Assert.Equal(vehicleId1.GetHashCode(), vehicleId2.GetHashCode());
        }

        [Fact]
        public void NotBeEqualWhenComparingTwoVehicleIdsWithDifferentValues()
        {
            // Given (Arrange)
            var vehicleId1 = new VehicleId(Guid.NewGuid());
            var vehicleId2 = new VehicleId(Guid.NewGuid());

            // When & Then (Act & Assert)
            Assert.NotEqual(vehicleId1, vehicleId2);
            Assert.False(vehicleId1 == vehicleId2);
            Assert.True(vehicleId1 != vehicleId2);
        }

        [Fact]
        public void ReturnGuidStringWhenCallingToString()
        {
            // Given (Arrange)
            var guid = Guid.NewGuid();
            var vehicleId = new VehicleId(guid);

            // When (Act)
            var result = vehicleId.ToString();

            // Then (Assert)
            Assert.Equal(guid.ToString(), result);
        }

        [Fact]
        public void CreateWithNewGuidWhenCallingNew()
        {
            // Given & When (Arrange & Act)
            var vehicleId = VehicleId.New();

            // Then (Assert)
            Assert.NotNull(vehicleId);
            Assert.NotEqual(Guid.Empty, vehicleId.Value);
        }
    }
}
