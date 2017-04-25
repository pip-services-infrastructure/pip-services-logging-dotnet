using System;
using System.Threading.Tasks;

using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Logging.Models;
using PipServices.Data.File;
using System.Collections.Generic;

namespace PipServices.Logging.Persistence
{
    public class LoggingFilePersistence : ILoggingPersistence
    {
        // TODO: ???
        //private JsonFilePersister<LogMessageV1> _messagesPersister;
        //private JsonFilePersister<LogMessageV1> _errorMessagesPersister;

        public int MaxPageSize => throw new NotImplementedException();

        public int MaxErrorSize => throw new NotImplementedException();

        public int MaxTotalSize => throw new NotImplementedException();

        public List<LogMessageV1> Messages => throw new NotImplementedException();

        public List<LogMessageV1> ErrorMessages => throw new NotImplementedException();

        public Task ClearAsync(string correlationId)
        {
            throw new NotImplementedException();
        }

        public void Configure(ConfigParams config)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(string correlationId, LogMessageV1 message)
        {
            throw new NotImplementedException();
        }

        public Task<LogMessageV1[]> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            throw new NotImplementedException();
        }
    }
}
