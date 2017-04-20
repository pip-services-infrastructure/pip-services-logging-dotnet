using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PipServices.Commons.Data;
using PipServices.Logging.Models;
using PipServices.Net.Rest;

namespace PipServices.Logging.Clients
{
    public class LoggingRestClientV1 : RestClient, ILoggingClientV1
    {
        public Task ClearAsync(string correlationId)
        {
            throw new NotImplementedException();
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            throw new NotImplementedException();
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            throw new NotImplementedException();
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            throw new NotImplementedException();
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            throw new NotImplementedException();
        }
    }
}
