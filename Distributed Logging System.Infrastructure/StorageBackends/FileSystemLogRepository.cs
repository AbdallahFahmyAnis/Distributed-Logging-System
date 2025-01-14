using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Distributed_Logging_System.Domain.Entities;
using Distributed_Logging_System.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Distributed_Logging_System.Infrastructure.StorageBackends
{
    public class FileSystemLogRepository : ILogRepository
    {
        private readonly string _logDirectory;
        private readonly ILogger<FileSystemLogRepository> _logger;
        private static readonly object _fileLock = new();

        public FileSystemLogRepository(string logDirectory, ILogger<FileSystemLogRepository> logger)
        {
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), logDirectory);
            _logger = logger;

            // Log the path for debugging
            _logger.LogInformation("Log directory path: {LogDirectory}", _logDirectory);

            // Ensure directory exists
            if (!Directory.Exists(_logDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_logDirectory);
                    _logger.LogInformation("Created log directory: {LogDirectory}", _logDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create log directory: {LogDirectory}", _logDirectory);
                    throw new IOException($"Failed to create log directory: {_logDirectory}", ex);
                }
            }
        }

        public async Task StoreLogAsync(LogEntry logEntry)
        {
            if (logEntry == null)
                throw new ArgumentNullException(nameof(logEntry));

            // Specify the file name as log.json for appending logs to a single file
            var filePath = Path.Combine(_logDirectory, "log.json");

            var json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });

            lock (_fileLock)
            {
                try
                {
                    _logger.LogInformation("Attempting to store log entry to {FilePath}", filePath);

                    // If log.json already exists, append new log to it
                    if (File.Exists(filePath))
                    {
                        _logger.LogInformation("log.json exists. Appending to it.");
                        var existingLogs = File.ReadAllText(filePath);
                        var logs = JsonSerializer.Deserialize<List<LogEntry>>(existingLogs) ?? new List<LogEntry>();
                        logs.Add(logEntry);
                        var updatedJson = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(filePath, updatedJson);
                    }
                    else
                    {
                        _logger.LogInformation("log.json does not exist. Creating new file.");
                        var logList = new List<LogEntry> { logEntry };
                        var jsonToWrite = JsonSerializer.Serialize(logList, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(filePath, jsonToWrite);
                    }

                    _logger.LogInformation("Successfully stored log entry with ID: {LogEntryId} at {FilePath}", logEntry.Id, filePath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Access denied to write log entry to file: {FilePath}", filePath);
                    throw new IOException($"Access denied to write log entry to file: {filePath}", ex);
                }
                catch (PathTooLongException ex)
                {
                    _logger.LogError(ex, "File path is too long: {FilePath}", filePath);
                    throw new IOException($"File path is too long: {filePath}", ex);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Failed to store log entry with ID: {LogEntryId}", logEntry.Id);
                    throw new IOException($"Failed to store log entry with ID: {logEntry.Id}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while storing log entry with ID: {LogEntryId}", logEntry.Id);
                    throw new IOException($"An unexpected error occurred while storing log entry with ID: {logEntry.Id}", ex);
                }
            }
        }

        public async Task<List<LogEntry>> RetrieveLogsAsync(string service, string level, DateTime? startTime, DateTime? endTime)
        {
            if (startTime.HasValue && endTime.HasValue && startTime > endTime)
                throw new ArgumentException("Start time cannot be greater than end time.");

            var logs = new List<LogEntry>();
            var logFiles = Directory.GetFiles(_logDirectory, "*.json");

            var tasks = logFiles.Select(async file =>
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var logEntry = JsonSerializer.Deserialize<LogEntry>(json);

                    if (logEntry != null &&
                        (string.IsNullOrEmpty(service) || logEntry.Service == service) &&
                        (string.IsNullOrEmpty(level) || logEntry.Level.ToString() == level) &&
                        (!startTime.HasValue || logEntry.Timestamp >= startTime.Value) &&
                        (!endTime.HasValue || logEntry.Timestamp <= endTime.Value))
                    {
                        lock (logs)
                        {
                            logs.Add(logEntry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read or parse log file: {File}", file);
                }
            });

            await Task.WhenAll(tasks);

            return logs;
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                    return false;

                var testFilePath = Path.Combine(_logDirectory, "healthcheck.tmp");
                await File.WriteAllTextAsync(testFilePath, "test");

                var content = await File.ReadAllTextAsync(testFilePath);
                File.Delete(testFilePath);

                return content == "test";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed for log directory: {LogDirectory}", _logDirectory);
                return false;
            }
        }

        public async Task CleanupOldLogsAsync(TimeSpan retentionPeriod)
        {
            var cutoffTime = DateTime.UtcNow - retentionPeriod;
            var logFiles = Directory.GetFiles(_logDirectory, "*.json");

            foreach (var file in logFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTimeUtc < cutoffTime)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete log file: {File}", file);
                }
            }
        }
    }
}