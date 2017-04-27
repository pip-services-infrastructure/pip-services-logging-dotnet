using System.Threading.Tasks;
using System.Net.Http;

using PipServices.Commons.Data;
using PipServices.Logging.Models;
using PipServices.Net.Rest;

namespace PipServices.Logging.Clients
{
    public class LoggingHttpClientV1 : RestClient, ILoggingClientV1
    {
        public Task ClearAsync(string correlationId)
        {
            using (var timing = Instrument(correlationId))
            {
                return ExecuteAsync(correlationId, HttpMethod.Post, 
                    $"logging?correlation_id={correlationId}");
            }
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            using (var timing = Instrument(correlationId))
            {
                return ExecuteAsync<LogMessageV1[]>(correlationId, HttpMethod.Get,
                    $"logging/errors?correlation_id={correlationId}&filter={filter}&paging={paging}");
            }
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            using (var timing = Instrument(correlationId))
            {
                return ExecuteAsync<LogMessageV1[]>(correlationId, HttpMethod.Get,
                    $"logging/messages?correlation_id={correlationId}&filter={filter}&paging={paging}");
            }
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            using (var timing = Instrument(correlationId))
            {
                return ExecuteAsync(correlationId, HttpMethod.Post,
                    $"logging?correlation_id={correlationId}", message);
            }
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            using (var timing = Instrument(correlationId))
            {
                return ExecuteAsync(correlationId, HttpMethod.Post,
                    $"logging?correlation_id={correlationId}", messages);
            }
        }
    }
}
