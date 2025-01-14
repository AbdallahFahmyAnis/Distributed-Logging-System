using Distributed_Logging_System.Domain.ENUMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Domain.Interfaces
{
    public interface ILogRepositoryFactory
    {
        ILogRepository Create(StorageType backendType);
    }
}
