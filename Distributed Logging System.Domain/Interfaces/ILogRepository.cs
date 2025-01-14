using Distributed_Logging_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Domain.Interfaces
{
    public interface ILogRepository
    {
        Task StoreLogAsync(LogEntry logEntry);
        Task<List<LogEntry>> RetrieveLogsAsync(string service, string level, DateTime? startTime, DateTime? endTime);
        Task<bool> CheckHealthAsync();
    }
}
