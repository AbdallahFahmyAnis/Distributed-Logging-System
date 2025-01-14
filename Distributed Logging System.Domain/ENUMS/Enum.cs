using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed_Logging_System.Domain.ENUMS
{
    public enum StorageType
    {
        MongoDB,      // Represents MongoDB
        S3,           // Represents Amazon S3
        FileSystem,   // Represents a file system
        RabbitMQ,
        Buffer// Represents RabbitMQ
    }


    public enum LogLevel
    {
        Debug,        // Debug level
        Info,         // Information level
        Warning,      // Warning level
        Error,        // Error level
        Fatal         // Fatal level
    }
}
