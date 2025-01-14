using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Infrastructure.StorageBackends
{
    public class S3LogRepository : ILogRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _bucketName;

        public S3LogRepository(HttpClient httpClient, string bucketName)
        {
            _httpClient = httpClient;
            _bucketName = bucketName;
        }
        public async Task StoreLogAsync(LogEntry logEntry)
        {
            logEntry.StorageBackend = "S3bucket";

            // Serialize the log entry to JSON
            var json = JsonSerializer.Serialize(logEntry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send a PUT request to S3 to store the log
            var response = await _httpClient.PutAsync($"{_bucketName}/{logEntry.Id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<LogEntry>> RetrieveLogsAsync(string service, string level, DateTime? startTime, DateTime? endTime)
        {
            // Send a GET request to S3 to retrieve logs based on filters
            var response = await _httpClient.GetAsync($"{_bucketName}?service={service}&level={level}&startTime={startTime}&endTime={endTime}");
            response.EnsureSuccessStatusCode();

            // Deserialize the response to a list of log entries
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LogEntry>>(json);
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                // Send a HEAD request to check if the S3 bucket is accessible
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _bucketName));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
