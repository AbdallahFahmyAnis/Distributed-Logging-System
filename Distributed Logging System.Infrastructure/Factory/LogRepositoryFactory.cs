using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;
using Distributed_Logging_System.Infrastructure.Persistence;
using Distributed_Logging_System.Infrastructure.StorageBackends;
using DistributedLoggingSystem.Infrastructure.MessageQueue;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Infrastructure.Factory
{


    public class LogRepositoryFactory : ILogRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public LogRepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILogRepository Create(StorageType backendType)
        {
            return backendType switch
            {
                StorageType.MongoDB => _serviceProvider.GetRequiredService<MongoLogRepository>(),
                StorageType.FileSystem => _serviceProvider.GetRequiredService<FileSystemLogRepository>(),
                StorageType.RabbitMQ => _serviceProvider.GetRequiredService<MessageQueueLogRepository>(),
                StorageType.Buffer => _serviceProvider.GetRequiredService<BufferLogRepository>(),
                _ => throw new ArgumentOutOfRangeException(nameof(backendType), backendType, null)
            };
        }
    }
}