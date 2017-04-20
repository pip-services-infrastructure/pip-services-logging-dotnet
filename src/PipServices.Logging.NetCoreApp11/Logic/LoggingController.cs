using System.Threading.Tasks;

using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Commons.Errors;
using PipServices.Commons.Refer;
using PipServices.Logging.Memory;
using PipServices.Logging.Models;

namespace PipServices.Logging.Logic
{
    public class LoggingController : ILoggingBusinessLogic, IReferenceable, IConfigurable
    {
        private ILoggingMemoryPersistence _readPersistence;
        private ILoggingMemoryPersistence _writePersistence;

        public void SetReferences(IReferences references)
        {
            references.Put("read_persistence", new Descriptor("pip-services-logging", "persistence", "*", "*", "*"));
            references.Put("write_persistence", new Descriptor("pip-services-logging", "persistence", "*", "*", "*"));

            _readPersistence = references.GetOneRequired<ILoggingMemoryPersistence>("read_persistence");
            _writePersistence = references.GetOneOptional<ILoggingMemoryPersistence>("write_persistence");

            if (_readPersistence == null)
            {
                throw new ConfigException(null, "NO_PERSISTENCE", "Read Logging Memory Persistance is not configured");
            }

            if (_writePersistence == null)
            {
                throw new ConfigException(null, "NO_PERSISTENCE", "Write Logging Memory Persistance is not configured");
            }
        }

        public void Configure(ConfigParams config)
        {
            _readPersistence.Configure(config);
            _writePersistence.Configure(config);
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            return _readPersistence.GetPageByFilterAsync(correlationId, filter, paging);
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter ?? new FilterParams();
            filter.SetAsObject("errors_only", true);

            return _readPersistence.GetPageByFilterAsync(correlationId, filter, paging);
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            return _writePersistence.CreateAsync(correlationId, message);
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            return Task.Run( ()=> 
            {
                foreach (var message in messages)
                {
                    _writePersistence.CreateAsync(correlationId, message);
                }
            } );
        }

        public Task ClearAsync(string correlationId)
        {
            return _writePersistence.ClearAsync(correlationId);
        }
    }
}
