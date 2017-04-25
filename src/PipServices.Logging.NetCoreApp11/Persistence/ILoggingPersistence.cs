using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Commons.Run;
using PipServices.Logging.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipServices.Logging.Persistence
{
    public interface ILoggingPersistence : ICleanable, IConfigurable
    {
        int MaxPageSize { get; }
        int MaxErrorSize { get; }
        int MaxTotalSize { get; }

        List<LogMessageV1> Messages { get; }
        List<LogMessageV1> ErrorMessages { get; }

        Task<LogMessageV1[]> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task CreateAsync(string correlationId, LogMessageV1 message);
    }
}
