using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Infrastructure.Persistence
{
    public class MongoLogRepository : ILogRepository
    {
        private readonly IMongoCollection<LogEntry> _logsCollection;

        public MongoLogRepository(IMongoDatabase database)
        {
            _logsCollection = database.GetCollection<LogEntry>("logs");
        }

        public async Task StoreLogAsync(LogEntry logEntry)
        {

            logEntry.StorageBackend = "MongoDB";
            await _logsCollection.InsertOneAsync(logEntry);
        }
        public async Task<List<LogEntry>> GetStoreLogAsync()
        {
            // Assuming _logsCollection is of type IMongoCollection<LogEntry>
            var logs = await _logsCollection.FindAsync(new BsonDocument());

            // Convert the cursor to a list of LogEntry objects asynchronously
            return await logs.ToListAsync();
        }

        public async Task<List<LogEntry>> RetrieveLogsAsync(
            string service = null,
            string level = null,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            // Start with an empty filter
            var filter = Builders<LogEntry>.Filter.Empty;

            // Apply filters conditionally
            if (!string.IsNullOrEmpty(service))
            {
                filter &= Builders<LogEntry>.Filter.Eq(x => x.Service, service);
            }

            if (!string.IsNullOrEmpty(level))
            {
                filter &= Builders<LogEntry>.Filter.Eq(x => x.Level, level);
            }


            if (startTime.HasValue)
            {
                filter &= Builders<LogEntry>.Filter.Gte(x => x.Timestamp, startTime.Value);
            }

            if (endTime.HasValue)
            {
                filter &= Builders<LogEntry>.Filter.Lte(x => x.Timestamp, endTime.Value);
            }

            // Retrieve logs based on the filter
            return await _logsCollection.Find(filter).ToListAsync();
        }


        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                await _logsCollection.Database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}