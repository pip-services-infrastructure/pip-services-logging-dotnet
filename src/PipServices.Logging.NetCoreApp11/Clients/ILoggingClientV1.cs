using PipServices.Commons.Data;
using PipServices.Logging.Models;

using System.Threading.Tasks;

namespace PipServices.Logging.Clients
{
    public interface ILoggingClientV1
    {
        Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task WriteMessageAsync(string correlationId, LogMessageV1 message);
        Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages);
        Task ClearAsync(string correlationId);
    }
}
