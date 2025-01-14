using Distributed_Logging_System.Domain.ENUMS;
using Microsoft.Extensions.DependencyInjection;

namespace Distributed_Logging_System.Domain.Interfaces
{

    public interface IHealthCheckService
    {
        Task<Dictionary<StorageType, bool>> GetHealthStatusAsync();
    }
    
}