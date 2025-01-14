using Distributed_Logging_System.Application.UseCases;
using DistributedLoggingSystem.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Application.NewFolder
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register use cases
            services.AddScoped<StoreLogUseCase>();
            services.AddScoped<RetrieveLogsUseCase>();
            services.AddScoped<UserService>();

            return services;
        }
    }
}
