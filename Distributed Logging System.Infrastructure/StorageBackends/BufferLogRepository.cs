using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Infrastructure.StorageBackends
{
    public class BufferLogRepository : ILogRepository
    {
        private readonly List<LogEntry> _buffer = new();

        public Task StoreLogAsync(LogEntry logEntry)
        {
            _buffer.Add(logEntry);
            return Task.CompletedTask;
        }

        public Task<List<LogEntry>> RetrieveLogsAsync(string service, string level, DateTime? startTime, DateTime? endTime)
        {
            var logs = _buffer.Where(log =>
                (string.IsNullOrEmpty(service) || log.Service == service) &&
                (string.IsNullOrEmpty(level) || log.Level.ToString() == level) &&
                (!startTime.HasValue || log.Timestamp >= startTime.Value) &&
                (!endTime.HasValue || log.Timestamp <= endTime.Value)
            ).ToList();

            return Task.FromResult(logs);
        }

        public Task<bool> CheckHealthAsync()
        {
            // Buffer is always healthy
            return Task.FromResult(true);
        }

        public async Task FlushAsync(ILogRepository targetRepository)
        {
            foreach (var logEntry in _buffer)
            {
                await targetRepository.StoreLogAsync(logEntry);
            }
            _buffer.Clear();
        }
    }
}