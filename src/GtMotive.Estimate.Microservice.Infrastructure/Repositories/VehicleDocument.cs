#nullable enable

using System;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Enums;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GtMotive.Estimate.Microservice.Infrastructure.Repositories
{
    internal class VehicleDocument
    {
        public VehicleDocument()
        {
        }

        public VehicleDocument(Vehicle vehicle)
        {
            Id = vehicle.Id.Value.ToString();
            LicensePlate = vehicle.LicensePlate.Value;
            ManufacturingDate = vehicle.ManufacturingDate.Value;
            Model = vehicle.Model;
            Brand = vehicle.Brand;
            Status = vehicle.Status;
            CustomerId = vehicle.CurrentCustomerId;
        }

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("licensePlate")]
        public string LicensePlate { get; set; } = string.Empty;

        [BsonElement("manufacturingDate")]
        public DateTime ManufacturingDate { get; set; }

        [BsonElement("model")]
        public string Model { get; set; } = string.Empty;

        [BsonElement("brand")]
        public string Brand { get; set; } = string.Empty;

        [BsonElement("status")]
        public VehicleStatus Status { get; set; }

        [BsonElement("customerId")]
        public string? CustomerId { get; set; }

        public Vehicle ToVehicle()
        {
            var vehicle = new Vehicle(
                new VehicleId(Guid.Parse(Id)),
                new LicensePlate(LicensePlate),
                new ManufacturingDate(ManufacturingDate),
                Model,
                Brand);

            if (Status == VehicleStatus.Rented && !string.IsNullOrEmpty(CustomerId))
            {
                vehicle.Rent(CustomerId);
            }

            return vehicle;
        }
    }
}
