using System.Threading.Tasks;

using PipServices.Commons.Data;
using PipServices.Logging.Models;
using PipServices.Net.Rest;

namespace PipServices.Logging.Clients
{
    public class LoggingHttpClientV1 : CommandableHttpClient, ILoggingClientV1
    {
        public LoggingHttpClientV1() 
            : base("logging")
        {
        }

        public Task ClearAsync(string correlationId)
        {
            using (var timing = Instrument(correlationId))
            {
                var requestEntity = new
                {
                    correlation_id = correlationId
                };

                return CallCommand<Task>("clear", correlationId, requestEntity);
            }
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            using (var timing = Instrument(correlationId))
            {
                filter = filter ?? new FilterParams();
                paging = paging ?? new PagingParams();

                var requestEntity = new
                {
                    correlation_id = correlationId,
                    filter,
                    paging
                };

                return CallCommand<LogMessageV1[]>("read_errors", correlationId, requestEntity);
            }
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            using (var timing = Instrument(correlationId))
            {
                filter = filter ?? new FilterParams();
                paging = paging ?? new PagingParams();

                var requestEntity = new
                {
                    correlation_id = correlationId,
                    filter,
                    paging
                };

                return CallCommand<LogMessageV1[]>("read_messages", correlationId, requestEntity);
            }
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            using (var timing = Instrument(correlationId))
            {
                var requestEntity = new
                {
                    correlation_id = correlationId,
                    message
                };

                return CallCommand<Task>("write_message", correlationId, requestEntity);
            }
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            using (var timing = Instrument(correlationId))
            {
                var requestEntity = new
                {
                    correlation_id = correlationId,
                    messages
                };

                return CallCommand<Task>("write_messages", correlationId, requestEntity);
            }
        }
    }
}
