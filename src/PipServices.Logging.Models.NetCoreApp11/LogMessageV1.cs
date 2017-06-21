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
        public string Correlation_Id { get; private set; }
        public ErrorDescription Error { get; private set; }
        public string Message { get; private set; }

        public LogMessageV1()
            : this(DateTime.UtcNow, LogLevel.None, string.Empty, string.Empty, null, string.Empty)
        {
        }

        public LogMessageV1(LogLevel level, string source, string correlationId, ErrorDescription error, string message)
            : this(DateTime.UtcNow, level, source, correlationId, error, message)
        {
        }

        public LogMessageV1(DateTime datetime, LogLevel level, string source, string correlationId, ErrorDescription error, string message)
        {
            Time = datetime;
            Level = level;
            Source = source;
            Correlation_Id = correlationId;
            Error = error;
            Message = message;
        }

        public override bool Equals(object obj)
        {
            var logMessage = obj as LogMessageV1;

            return logMessage != null &&
                Time == logMessage.Time &&
                Level == logMessage.Level &&
                Source == logMessage.Source &&
                Correlation_Id == logMessage.Correlation_Id &&
                Message == logMessage.Message;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}