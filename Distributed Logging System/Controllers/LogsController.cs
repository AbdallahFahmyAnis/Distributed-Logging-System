using Distributed_Logging_System.Application.UseCases;
using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.ENUMS;
using Distributed_Logging_System.Domain.Interfaces;
using DistributedLoggingSystem.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Controllers
{
    [Authorize]
    [ApiController]
    [Route("v1/logs")]
    public class LogsController : ControllerBase
    {
        private readonly StoreLogUseCase _storeLogUseCase;
        private readonly RetrieveLogsUseCase _retrieveLogsUseCase;
        private readonly ILogRepositoryFactory _storageFactory;

        public LogsController(
            StoreLogUseCase storeLogUseCase,
            RetrieveLogsUseCase retrieveLogsUseCase,
            ILogRepositoryFactory storageFactory)
        {
            _storeLogUseCase = storeLogUseCase;
            _retrieveLogsUseCase = retrieveLogsUseCase;
            _storageFactory = storageFactory;
        }

        /// <summary>
        /// Stores a log entry using LoadBalancing and healthy cheaks
        /// </summary>
        [HttpPost("Automated")]
        public async Task<IActionResult> StoreLog([FromQuery] Domain.ENUMS.LogLevel? level  , [FromBody] LogEntryDto logEntry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var model = new LogEntry
            {

                Service = logEntry.Service,
                Level = level.ToString(),
                Message = logEntry.Message

            };
            await _storeLogUseCase.Execute(model);
            return Ok();
        }

        /// <summary>
        /// Add logs with optional  service
        /// </summary>
        [HttpPost("dynamic")]
        public async Task<IActionResult> StoreLogDynamic([FromBody] LogEntry logEntry, [FromQuery] StorageType backendType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var storage = _storageFactory.Create(backendType);
            await storage.StoreLogAsync(logEntry);

            return Ok();
        }

       

        /// <summary>
        /// Retrieves logs with optional filters for service, level, and time range.
        /// </summary>
        [HttpGet()]
        public async Task<IActionResult> GetLogs(
            [FromQuery] string service = null,
            [FromQuery] Domain.ENUMS.LogLevel? level = null,
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null)
        {
            var logs = await _retrieveLogsUseCase.Execute(service, level.ToString(), startTime, endTime);
            return Ok(logs);
        }
    }
}