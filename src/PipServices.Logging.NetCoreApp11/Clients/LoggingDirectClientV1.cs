using PipServices.Commons.Refer;
using PipServices.Net.Direct;
using PipServices.Logging.Logic;
using PipServices.Commons.Data;
using PipServices.Logging.Models;

using System.Threading.Tasks;

namespace PipServices.Logging.Clients
{
    public class LoggingDirectClientV1 : DirectClient<ILoggingController>, ILoggingClientV1
    {
        public LoggingDirectClientV1()
        {
            _dependencyResolver.Put("controller", new Descriptor("pip-services-logging", "controller", "*", "*", "*"));
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            using (var timing = Instrument(correlationId))
            {
                return _controller.ReadErrorsAsync(correlationId, filter, paging);
            }
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            using (var timing = Instrument(correlationId))
            {
                return _controller.ReadMessagesAsync(correlationId, filter, paging);
            }
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            using (var timing = Instrument(correlationId))
            {
                return _controller.WriteMessageAsync(correlationId, message);
            }
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            using (var timing = Instrument(correlationId))
            {
                return _controller.WriteMessagesAsync(correlationId, messages);
            }
        }

        public Task ClearAsync(string correlationId)
        {
            using (var timing = Instrument(correlationId))
            {
                return _controller.ClearAsync(correlationId);
            }
        }

    }
}
