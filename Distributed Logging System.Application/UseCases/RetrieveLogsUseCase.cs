using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedLoggingSystem.Application.UseCases
{
    public class RetrieveLogsUseCase
    {
        private readonly ILogRepositoryFactory _repositoryFactory;
        private readonly IHealthCheckService _healthCheckService;

        public RetrieveLogsUseCase(ILogRepositoryFactory repositoryFactory, IHealthCheckService healthCheckService)
        {
            _repositoryFactory = repositoryFactory;
            _healthCheckService = healthCheckService;
        }

        public async Task<List<LogEntry>> Execute(string service, string level, DateTime? startTime, DateTime? endTime)
        {
            var repository = await GetNextHealthyRepositoryAsync();
            return await repository.RetrieveLogsAsync(service, level, startTime, endTime);
        }

        private async Task<ILogRepository> GetNextHealthyRepositoryAsync()
        {
            var healthStatus = await _healthCheckService.GetHealthStatusAsync();
            var healthyRepos = healthStatus
                .Where(x => x.Value && Enum.IsDefined(typeof(StorageType), x.Key)) // Ensure valid StorageType
                .Select(x => x.Key)
                .ToList();

            if (healthyRepos.Count == 0)
            {
                // Fallback to buffer if no healthy services are available
                return _repositoryFactory.Create(StorageType.Buffer);
            }

            // Round-robin logic
            var nextRepoType = healthyRepos[new Random().Next(healthyRepos.Count)];
            return _repositoryFactory.Create(nextRepoType);
        }
    }
}