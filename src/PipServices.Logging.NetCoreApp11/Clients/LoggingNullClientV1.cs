using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PipServices.Commons.Data;
using PipServices.Logging.Models;

namespace PipServices.Logging.Clients
{
    public class LoggingNullClientV1 : ILoggingClientV1
    {
        public Task ClearAsync(string correlationId)
        {
            return DoNothingAsync();
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            return GetEmptyArrayAsync();
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            return GetEmptyArrayAsync();
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            return DoNothingAsync();
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            return DoNothingAsync();
        }

        private Task DoNothingAsync()
        {
            return Task.Delay(0);
        }

        private Task<LogMessageV1[]> GetEmptyArrayAsync()
        {
            return Task.Run(() => { return new List<LogMessageV1>().ToArray(); });
        }
    }
}
