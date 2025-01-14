using Distributed_Logging_System.Domain.ENUMS;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Domain.Interfaces
{
    public interface IDistributionStrategy
    {
        Task<ILogRepository> GetNextRepositoryAsync();
    }

    public class DistributionService
    {
        private readonly ILogRepositoryFactory _repositoryFactory;
        private readonly IHealthCheckService _healthCheckService;
        private int _currentIndex = 0;

        public DistributionService(ILogRepositoryFactory repositoryFactory, IHealthCheckService healthCheckService)
        {
            _repositoryFactory = repositoryFactory;
            _healthCheckService = healthCheckService;
        }

        public async Task<ILogRepository> GetNextRepositoryAsync()
        {
            var healthStatus = await _healthCheckService.GetHealthStatusAsync();
            var healthyRepos = healthStatus.Where(x => x.Value).Select(x => x.Key).ToList();

            if (healthyRepos.Count == 0)
            {
                // Fallback to buffer if no healthy services are available
                return _repositoryFactory.Create(StorageType.Buffer);
            }

            // Round-robin logic
            var nextRepoType = healthyRepos[_currentIndex % healthyRepos.Count];
            _currentIndex++;

            return _repositoryFactory.Create(nextRepoType);
        }
    }
}
