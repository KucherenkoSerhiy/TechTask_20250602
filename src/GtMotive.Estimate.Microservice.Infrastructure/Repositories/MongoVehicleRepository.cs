#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Entities;
using GtMotive.Estimate.Microservice.Domain.Enums;
using GtMotive.Estimate.Microservice.Domain.Repositories;
using GtMotive.Estimate.Microservice.Domain.ValueObjects;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.Repositories
{
    public class MongoVehicleRepository : IVehicleRepository
    {
        private readonly IMongoCollection<VehicleDocument> _collection;

        public MongoVehicleRepository(MongoService mongoService, IOptions<MongoDbSettings> settings)
        {
            if (mongoService == null)
            {
                throw new ArgumentNullException(nameof(mongoService));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var database = mongoService.MongoClient.GetDatabase(settings.Value.MongoDbDatabaseName);
            _collection = database.GetCollection<VehicleDocument>("vehicles");
        }

        public async Task<Vehicle?> GetByIdAsync(VehicleId vehicleId, CancellationToken cancellationToken = default)
        {
            if (vehicleId == null)
            {
                throw new ArgumentNullException(nameof(vehicleId));
            }

            var filter = Builders<VehicleDocument>.Filter.Eq(x => x.Id, vehicleId.Value.ToString());
            var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            return document?.ToVehicle();
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken = default)
        {
            if (licensePlate == null)
            {
                throw new ArgumentNullException(nameof(licensePlate));
            }

            var filter = Builders<VehicleDocument>.Filter.Eq(x => x.LicensePlate, licensePlate.Value);
            var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            return document?.ToVehicle();
        }

        public async Task<Vehicle?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<VehicleDocument>.Filter.Eq(x => x.CustomerId, customerId);
            var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            return document?.ToVehicle();
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(CancellationToken cancellationToken = default)
        {
            var filter = Builders<VehicleDocument>.Filter.Eq(x => x.Status, VehicleStatus.Available);
            var documents = await _collection.Find(filter).ToListAsync(cancellationToken);
            return documents.Select(d => d.ToVehicle()).ToList();
        }

        public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            var document = new VehicleDocument(vehicle);
            await _collection.InsertOneAsync(document, null, cancellationToken);
        }

        public async Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            var filter = Builders<VehicleDocument>.Filter.Eq(x => x.Id, vehicle.Id.Value.ToString());
            var document = new VehicleDocument(vehicle);
            await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions(), cancellationToken);
        }

        public async Task<bool> ExistsWithLicensePlateAsync(LicensePlate licensePlate, CancellationToken cancellationToken = default)
        {
            if (licensePlate == null)
            {
                throw new ArgumentNullException(nameof(licensePlate));
            }

            var filter = Builders<VehicleDocument>.Filter.Eq(x => x.LicensePlate, licensePlate.Value);
            var count = await _collection.CountDocumentsAsync(filter, null, cancellationToken);
            return count > 0;
        }
    }
}
