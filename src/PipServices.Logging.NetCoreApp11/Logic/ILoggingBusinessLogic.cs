using PipServices.Commons.Data;
using PipServices.Logging.Models;
using PipServices.Logging.Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipServices.Logging.Logic
{
    public interface ILoggingBusinessLogic
    {
        ILoggingPersistence WritePersistence { get; }
        ILoggingPersistence ReadPersistence { get; }

        Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task WriteMessageAsync(string correlationId, LogMessageV1 message);
        Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages);
        Task ClearAsync(string correlationId);
    }
}
