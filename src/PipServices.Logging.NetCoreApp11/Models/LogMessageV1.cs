using System;

using PipServices.Commons.Log;
using PipServices.Commons.Errors;

namespace PipServices.Logging.Models
{
    public class LogMessageV1
    {
        public DateTime Time { get; private set; }
        public string Source { get; private set; }
        public LogLevel Level { get; private set; }
        public string CorrelationId { get; private set; }
        public ErrorDescription Error { get; private set; }
        public string Message { get; private set; }

        public LogMessageV1(LogLevel level, string source, string correlationId, ErrorDescription error, string message)
        {
            Time = DateTime.UtcNow;
            Level = level;
            Source = source;
            CorrelationId = correlationId;
            Error = error;
            Message = message;
        }
    }
}