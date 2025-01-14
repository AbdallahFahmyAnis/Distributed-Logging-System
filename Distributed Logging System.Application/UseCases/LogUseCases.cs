using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Distributed_Logging_System.Application.UseCases
{
    public class StoreLogUseCase
    {
        private readonly ILogRepositoryFactory _repositoryFactory;
        private readonly IHealthCheckService _healthCheckService;
        private static int _roundRobinIndex = 0; // Static to maintain round-robin across instances

        public StoreLogUseCase(ILogRepositoryFactory repositoryFactory, IHealthCheckService healthCheckService)
        {
            _repositoryFactory = repositoryFactory;
            _healthCheckService = healthCheckService;
        }

        public async Task Execute(LogEntry logEntry)
        {
            try
            {
                var repository = await GetNextHealthyRepositoryAsync();
                await repository.StoreLogAsync(logEntry);
            }
            catch (Exception ex)
            {
                // Handle all exceptions by saving to the buffer
                Console.WriteLine($"Failed to store log in healthy repositories. Exception: {ex.Message}");
                var bufferRepository = _repositoryFactory.Create(StorageType.Buffer);
                await bufferRepository.StoreLogAsync(logEntry);
            }
        }

        private async Task<ILogRepository> GetNextHealthyRepositoryAsync()
        {
            var healthStatus = await _healthCheckService.GetHealthStatusAsync();
            var healthyRepos = healthStatus.Where(x => x.Value).Select(x => x.Key).ToList();

            if (!healthyRepos.Any())
            {
                // Fallback to buffer if no healthy repositories are available
                Console.WriteLine("No healthy repositories available. Falling back to buffer.");
                return _repositoryFactory.Create(StorageType.Buffer);
            }

            // Round-robin logic for distributing the load among healthy repositories
            var nextRepoType = healthyRepos[_roundRobinIndex % healthyRepos.Count];
            _roundRobinIndex++; // Move to the next repository in the list
            return _repositoryFactory.Create(nextRepoType);
        }
    }
}