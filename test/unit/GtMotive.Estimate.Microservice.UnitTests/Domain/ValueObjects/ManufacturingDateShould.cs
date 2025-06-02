using System;
using System.Globalization;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain.ValueObjects
{
    /// <summary>
    /// Tests for ManufacturingDate value object.
    /// BDD Scenario: Manufacturing date validation with 5-year business rule.
    /// </summary>
    public class ManufacturingDateShould
    {
        [Fact]
        public void CreateSuccessfullyWhenGivenValidManufacturingDateWithinFiveYears()
        {
            // Given (Arrange)
            var validDate = DateTime.Now.AddYears(-3); // 3 years old

            // When (Act)
            var manufacturingDate = new ManufacturingDate(validDate);

            // Then (Assert)
            Assert.NotNull(manufacturingDate);
            Assert.Equal(validDate.Date, manufacturingDate.Value.Date);
        }

        [Fact]
        public void CreateSuccessfullyWhenGivenManufacturingDateExactlyFiveYearsOld()
        {
            // Given (Arrange)
            var exactlyFiveYears = DateTime.Now.AddYears(-5);

            // When (Act)
            var manufacturingDate = new ManufacturingDate(exactlyFiveYears);

            // Then (Assert)
            Assert.NotNull(manufacturingDate);
            Assert.Equal(exactlyFiveYears.Date, manufacturingDate.Value.Date);
        }

        [Fact]
        public void ThrowDomainExceptionWhenGivenManufacturingDateOlderThanFiveYears()
        {
            // Given (Arrange)
            var olderThanFiveYears = DateTime.Now.AddYears(-6); // 6 years old

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new ManufacturingDate(olderThanFiveYears));
            Assert.Equal("Vehicle manufacturing date cannot be older than 5 years", exception.Message);
        }

        [Fact]
        public void ThrowDomainExceptionWhenGivenFutureDateAsManufacturingDate()
        {
            // Given (Arrange)
            var futureDate = DateTime.Now.AddDays(1);

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new ManufacturingDate(futureDate));
            Assert.Equal("Vehicle manufacturing date cannot be in the future", exception.Message);
        }

        [Fact]
        public void CreateSuccessfullyWhenGivenTodayAsManufacturingDate()
        {
            // Given (Arrange)
            var today = DateTime.Now;

            // When (Act)
            var manufacturingDate = new ManufacturingDate(today);

            // Then (Assert)
            Assert.NotNull(manufacturingDate);
            Assert.Equal(today.Date, manufacturingDate.Value.Date);
        }

        [Fact]
        public void BeEqualWhenComparingTwoManufacturingDatesWithSameValue()
        {
            // Given (Arrange)
            var date = DateTime.Now.AddYears(-2);
            var date1 = new ManufacturingDate(date);
            var date2 = new ManufacturingDate(date);

            // When & Then (Act & Assert)
            Assert.Equal(date1, date2);
            Assert.True(date1 == date2);
            Assert.False(date1 != date2);
            Assert.Equal(date1.GetHashCode(), date2.GetHashCode());
        }

        [Fact]
        public void NotBeEqualWhenComparingTwoManufacturingDatesWithDifferentValues()
        {
            // Given (Arrange)
            var date1 = new ManufacturingDate(DateTime.Now.AddYears(-1));
            var date2 = new ManufacturingDate(DateTime.Now.AddYears(-2));

            // When & Then (Act & Assert)
            Assert.NotEqual(date1, date2);
            Assert.False(date1 == date2);
            Assert.True(date1 != date2);
        }

        [Fact]
        public void ReturnDateStringWhenCallingToString()
        {
            // Given (Arrange)
            var date = DateTime.Now.AddYears(-2);
            var manufacturingDate = new ManufacturingDate(date);

            // When (Act)
            var result = manufacturingDate.ToString();

            // Then (Assert)
            Assert.Equal(date.Date.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture), result);
        }

        [Fact]
        public void ReturnCorrectResultWhenCheckingIfValidForFleet()
        {
            // Given (Arrange)
            var validDate = new ManufacturingDate(DateTime.Now.AddYears(-3));
            var borderlineDate = new ManufacturingDate(DateTime.Now.AddYears(-5));

            // When (Act)
            var isValidDateOk = validDate.IsValidForFleet();
            var isBorderlineDateOk = borderlineDate.IsValidForFleet();

            // Then (Assert)
            Assert.True(isValidDateOk);
            Assert.True(isBorderlineDateOk);
        }

        [Fact]
        public void ThrowDomainExceptionWhenGivenMinDateTime()
        {
            // Given (Arrange)
            var minDateTime = DateTime.MinValue;

            // When & Then (Act & Assert)
            var exception = Assert.Throws<DomainException>(() => new ManufacturingDate(minDateTime));
            Assert.Equal("Vehicle manufacturing date cannot be older than 5 years", exception.Message);
        }
    }
}
