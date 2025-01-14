using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Distributed_Logging_System.Application.UseCases;
using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.Interfaces;
using Distributed_Logging_System.Infrastructure.Factory;
using Distributed_Logging_System.Infrastructure.StorageBackends;
using DistributedLoggingSystem.Infrastructure.MessageQueue;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using Distributed_Logging_System.Infrastructure.Persistence;

namespace Distributed_Logging_System.Infrastructure.InfrastructureServiceCollectionExtensions
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register MongoDB
            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
                return client.GetDatabase("LogsDB");
            });

            // Register MongoLogRepository
            services.AddSingleton<MongoLogRepository>();
            services.AddSingleton<ILogRepository>(sp => sp.GetRequiredService<MongoLogRepository>());

            // Register FileSystemLogRepository
            services.AddSingleton<FileSystemLogRepository>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<FileSystemLogRepository>>();
                return new FileSystemLogRepository("Logs", logger);
            });
            services.AddSingleton<ILogRepository>(sp => sp.GetRequiredService<FileSystemLogRepository>());

            // Register RabbitMQ
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:HostName"],
                    UserName = configuration["RabbitMQ:UserName"],
                    Password = configuration["RabbitMQ:Password"]
                };

                try
                {
                    return factory.CreateConnection();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to connect to RabbitMQ", ex);
                }
            });

            // Register MessageQueueLogRepository
            services.AddSingleton<MessageQueueLogRepository>(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                var queueName = configuration["RabbitMQ:QueueName"]
                    ?? throw new InvalidOperationException("RabbitMQ QueueName is not configured.");
                return new MessageQueueLogRepository(connection, queueName);
            });
            services.AddSingleton<ILogRepository>(sp => sp.GetRequiredService<MessageQueueLogRepository>());

            // Register BufferLogRepository
            services.AddSingleton<BufferLogRepository>();
            services.AddSingleton<ILogRepository>(sp => sp.GetRequiredService<BufferLogRepository>());

            // Register Factory
            services.AddSingleton<ILogRepositoryFactory, LogRepositoryFactory>();

            // Register HealthCheckService
            services.AddSingleton<IHealthCheckService, HealthCheckService>();

            // Register MongoDB Identity
            var mongoDbSetting = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
                        mongoDbSetting.ConnectionString, mongoDbSetting.DatabaseName
                    );

            // Register UserService
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}