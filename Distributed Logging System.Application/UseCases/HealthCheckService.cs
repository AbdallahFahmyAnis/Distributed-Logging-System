using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Application.UseCases
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ILogRepositoryFactory _repositoryFactory;

        public HealthCheckService(ILogRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<Dictionary<StorageType, bool>> GetHealthStatusAsync()
        {
            var healthStatus = new Dictionary<StorageType, bool>();

            foreach (StorageType type in Enum.GetValues(typeof(StorageType)))
            {
                try
                {
                    Console.WriteLine($"Checking health for {type}...");
                    var repository = _repositoryFactory.Create(type);
                    healthStatus[type] = await repository.CheckHealthAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Health check failed for {type}: {ex.Message}");
                    healthStatus[type] = false; // Mark as unhealthy
                }
            }

            return healthStatus;
        }
    }
}
