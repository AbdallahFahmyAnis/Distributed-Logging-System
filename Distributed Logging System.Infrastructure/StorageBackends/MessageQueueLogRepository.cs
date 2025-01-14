using RabbitMQ.Client; // Ensure this is added

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.Interfaces;

namespace DistributedLoggingSystem.Infrastructure.MessageQueue
{

        public class MessageQueueLogRepository : ILogRepository
        {
            private readonly IConnection _connection;
            private readonly string _queueName;

            public MessageQueueLogRepository(IConnection connection, string queueName)
            {
                _connection = connection;
                _queueName = queueName;

                // Ensure the queue exists
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(
                        queue: _queueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                }
            }

            public async Task StoreLogAsync(LogEntry logEntry)
            {
                try
                {
                    logEntry.StorageBackend = "RabbitMQ";
                    var json = JsonSerializer.Serialize(logEntry);
                    var body = Encoding.UTF8.GetBytes(json);

                    using (var channel = _connection.CreateModel())
                    {
                        channel.BasicPublish(
                            exchange: "",
                            routingKey: _queueName,
                            basicProperties: null,
                            body: body
                        );
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to store log in RabbitMQ", ex);
                }
            }

            public Task<List<LogEntry>> RetrieveLogsAsync(string service, string level, DateTime? startTime, DateTime? endTime)
            {
                // Message queues are typically not used for retrieval
                throw new NotSupportedException("Retrieving logs is not supported for RabbitMQ.");
            }

            public Task<bool> CheckHealthAsync()
            {
                try
                {
                    using (var channel = _connection.CreateModel())
                    {
                        // Check if the connection is open
                        return Task.FromResult(_connection.IsOpen);
                    }
                }
                catch
                {
                    return Task.FromResult(false);
                }
            }
        }
    }
