using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Domain.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(string email, string fullName, string password);
        Task<string> LoginUserAsync(string email, string password);
    }
}
