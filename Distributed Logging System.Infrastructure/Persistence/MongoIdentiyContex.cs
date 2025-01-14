using Distributed_Logging_System.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Infrastructure.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            // Register the GUID serializer (only needed once during application startup)
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // Get MongoDB settings from configuration
            var mongoDbSettings = configuration.GetSection("MongoDbSettings");

            // Configure MongoDB client
            var clientSettings = MongoClientSettings.FromConnectionString(mongoDbSettings["ConnectionString"]);
            var client = new MongoClient(clientSettings);

            // Access the database
            _database = client.GetDatabase(mongoDbSettings["DatabaseName"]);
        }

        public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("Users");
        public IMongoCollection<ApplicationRole> Roles => _database.GetCollection<ApplicationRole>("Roles");
    }
}