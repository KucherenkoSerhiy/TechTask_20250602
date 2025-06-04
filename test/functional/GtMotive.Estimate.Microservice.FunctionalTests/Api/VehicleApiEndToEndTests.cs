#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Repositories;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Api
{
    /// <summary>
    /// End-to-end functional tests for the Vehicle API endpoints.
    /// Tests the complete HTTP workflow from API requests to database persistence.
    /// </summary>
    [Collection(TestCollections.Functional)]
    public class VehicleApiEndToEndTests : FunctionalTestBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public VehicleApiEndToEndTests(CompositionRootTestFixture fixture)
            : base(fixture)
        {
            _httpClient = fixture?.CreateClient() ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public async Task VehicleRentalWorkflowShouldWorkEndToEnd()
        {
            // Arrange - Create a test vehicle in the database
            var testVehicle = CreateTestVehicle();

            await Fixture.UsingRepository<IVehicleRepository>(async vehicleRepository =>
            {
                await vehicleRepository.AddAsync(testVehicle);
            });

            await Task.Delay(QueueWaitingTimeInMilliseconds); // Allow for any async processing

            var customerId = Guid.NewGuid();
            var rentRequest = new RentVehicleRequest
            {
                VehicleId = testVehicle.Id.Value.ToString(),
                CustomerId = customerId.ToString()
            };

            // Act & Assert - Rent the vehicle via HTTP API
            var rentResponse = await PostJsonAsync("/api/vehicle/rent", rentRequest);
            rentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var rentContent = await rentResponse.Content.ReadAsStringAsync();
            var rentedVehicle = JsonSerializer.Deserialize<VehicleDto>(rentContent, JsonOptions);
            rentedVehicle.Should().NotBeNull();
            rentedVehicle!.CurrentCustomerId.Should().Be(customerId.ToString());

            // Act & Assert - Get available vehicles (should not include rented vehicle)
            var availableResponse = await _httpClient.GetAsync(new Uri("/api/vehicle/available", UriKind.Relative));
            availableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var availableContent = await availableResponse.Content.ReadAsStringAsync();
            var availableVehicles = JsonSerializer.Deserialize<VehicleDto[]>(availableContent, JsonOptions);
            availableVehicles.Should().NotContain(v => v.Id == testVehicle.Id.Value.ToString());

            // Act & Assert - Get customer rental
            var customerRentalResponse = await _httpClient.GetAsync(new Uri($"/api/vehicle/customer/{customerId}/rental", UriKind.Relative));
            customerRentalResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var customerRentalContent = await customerRentalResponse.Content.ReadAsStringAsync();
            var customerVehicle = JsonSerializer.Deserialize<VehicleDto>(customerRentalContent, JsonOptions);
            customerVehicle.Should().NotBeNull();
            customerVehicle!.LicensePlate.Should().Be(testVehicle.LicensePlate.Value);

            // Act & Assert - Return the vehicle
            var returnRequest = new ReturnVehicleRequest
            {
                VehicleId = testVehicle.Id.Value.ToString(),
                CustomerId = customerId.ToString()
            };

            var returnResponse = await PostJsonAsync("/api/vehicle/return", returnRequest);
            returnResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var returnContent = await returnResponse.Content.ReadAsStringAsync();
            var returnedVehicle = JsonSerializer.Deserialize<VehicleDto>(returnContent, JsonOptions);
            returnedVehicle.Should().NotBeNull();
            returnedVehicle!.CurrentCustomerId.Should().BeNull();

            // Act & Assert - Verify vehicle is available again
            var finalAvailableResponse = await _httpClient.GetAsync(new Uri("/api/vehicle/available", UriKind.Relative));
            finalAvailableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var finalAvailableContent = await finalAvailableResponse.Content.ReadAsStringAsync();
            var finalAvailableVehicles = JsonSerializer.Deserialize<VehicleDto[]>(finalAvailableContent, JsonOptions);
            finalAvailableVehicles.Should().Contain(v => v.Id == testVehicle.Id.Value.ToString());
        }

        [Fact]
        public async Task RentNonExistentVehicleShouldReturnNotFound()
        {
            // Arrange
            var nonExistentVehicleId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var rentRequest = new RentVehicleRequest
            {
                VehicleId = nonExistentVehicleId.ToString(),
                CustomerId = customerId.ToString()
            };

            // Act
            var response = await PostJsonAsync("/api/vehicle/rent", rentRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCustomerRentalWhenNoRentalShouldReturnNotFound()
        {
            // Arrange
            var customerId = Guid.NewGuid();

            // Act
            var response = await _httpClient.GetAsync(new Uri($"/api/vehicle/customer/{customerId}/rental", UriKind.Relative));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RentVehicleWithInvalidDataShouldReturnBadRequest()
        {
            // Arrange
            var rentRequest = new RentVehicleRequest
            {
                VehicleId = "invalid-guid",
                CustomerId = Guid.NewGuid().ToString()
            };

            // Act
            var response = await PostJsonAsync("/api/vehicle/rent", rentRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private static Vehicle CreateTestVehicle(VehicleId? vehicleId = null, LicensePlate? licensePlate = null)
        {
            return new Vehicle(
                vehicleId ?? VehicleId.New(),
                licensePlate ?? new LicensePlate($"TEST{Guid.NewGuid().ToString()[..6]}"),
                new ManufacturingDate(DateTime.Now.AddYears(-2)),
                "Test Model",
                "Test Brand");
        }

        private async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(new Uri(url, UriKind.Relative), content);
        }
    }
}
