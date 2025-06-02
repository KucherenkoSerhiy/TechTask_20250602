using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.ValueObjects
{
    /// <summary>
    /// Tests for LicensePlate value object.
    /// BDD Scenario: Valid license plate creation and validation.
    /// </summary>
    public class LicensePlateShould
    {
        [Theory]
        [InlineData("ABC1234")]
        [InlineData("XYZ5678")]
        [InlineData("DEF9876")]
        public void CreateSuccessfullyWhenGivenValidLicensePlate(string validPlate)
        {
            // Given (Arrange) - validPlate parameter

            // When (Act)
            var licensePlate = new LicensePlate(validPlate);

            // Then (Assert)
            Assert.NotNull(licensePlate);
            Assert.Equal(validPlate?.ToUpperInvariant(), licensePlate.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowDomainExceptionWhenGivenNullOrEmptyLicensePlate(string invalidPlate)
        {
            // Given (Arrange) - invalidPlate parameter

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new LicensePlate(invalidPlate));
            Assert.Equal("License plate cannot be null or empty", exception.Message);
        }

        [Theory]
        [InlineData("AB")]
        [InlineData("ABCDEFGHIJKLMNOP")]
        public void ThrowDomainExceptionWhenGivenInvalidLengthLicensePlate(string invalidPlate)
        {
            // Given (Arrange) - invalidPlate parameter

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new LicensePlate(invalidPlate));
            Assert.Equal("License plate must be between 3 and 10 characters", exception.Message);
        }

        [Theory]
        [InlineData("ABC@123")]
        [InlineData("ABC 123")]
        [InlineData("ABC-123")]
        public void ThrowDomainExceptionWhenGivenLicensePlateWithInvalidCharacters(string invalidPlate)
        {
            // Given (Arrange) - invalidPlate parameter

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new LicensePlate(invalidPlate));
            Assert.Equal("License plate can only contain letters and numbers", exception.Message);
        }

        [Fact]
        public void BeEqualWhenComparingTwoLicensePlatesWithSameValue()
        {
            // Given (Arrange)
            var plate1 = new LicensePlate("ABC1234");
            var plate2 = new LicensePlate("abc1234"); // Different case

            // When & Then (Act & Assert)
            Assert.Equal(plate1, plate2);
            Assert.True(plate1 == plate2);
            Assert.False(plate1 != plate2);
            Assert.Equal(plate1.GetHashCode(), plate2.GetHashCode());
        }

        [Fact]
        public void NotBeEqualWhenComparingTwoLicensePlatesWithDifferentValues()
        {
            // Given (Arrange)
            var plate1 = new LicensePlate("ABC1234");
            var plate2 = new LicensePlate("XYZ5678");

            // When & Then (Act & Assert)
            Assert.NotEqual(plate1, plate2);
            Assert.False(plate1 == plate2);
            Assert.True(plate1 != plate2);
        }

        [Fact]
        public void ReturnValueWhenCallingToString()
        {
            // Given (Arrange)
            var plateValue = "ABC1234";
            var licensePlate = new LicensePlate(plateValue);

            // When (Act)
            var result = licensePlate.ToString();

            // Then (Assert)
            Assert.Equal(plateValue.ToUpperInvariant(), result);
        }

        [Fact]
        public void NormalizeToUpperCaseWhenCreatingWithLowerCaseLicensePlate()
        {
            // Given (Arrange)
            var lowerCasePlate = "abc1234";

            // When (Act)
            var licensePlate = new LicensePlate(lowerCasePlate);

            // Then (Assert)
            Assert.Equal("ABC1234", licensePlate.Value);
        }
    }
}
