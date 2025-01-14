using Distributed_Logging_System.Domain.ENUMS;

namespace Distributed_Logging_System.Domain.Entities
{
    // LogEntry.cs
    public class LogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Service { get; set; }
        public string Level { get; set; } // info, warning, error
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string StorageBackend { get; set; } // S3, MongoDB, FileSystem, MessageQueue
    }
    
    
    public class LogEntryDto
    {
        public string Service { get; set; }
        public string Message { get; set; }
    }
}
