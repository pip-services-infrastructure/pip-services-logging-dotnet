using System.Threading.Tasks;

using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Commons.Refer;
using PipServices.Logging.Persistence;
using PipServices.Logging.Models;

namespace PipServices.Logging.Logic
{
    public class LoggingController : ILoggingBusinessLogic, IReferenceable, IConfigurable
    {
        private readonly DependencyResolver _dependencyResolver;

        public ILoggingPersistence WritePersistence { get; private set; }
        public ILoggingPersistence ReadPersistence { get; private set; }

        public LoggingController()
        {
            _dependencyResolver = new DependencyResolver();

            _dependencyResolver.Put("read_persistence", new Descriptor("pip-services-logging", "persistence", "*", "*", "*"));
            _dependencyResolver.Put("write_persistence", new Descriptor("pip-services-logging", "persistence", "*", "*", "*"));
        }

        public void SetReferences(IReferences references)
        {
            _dependencyResolver.SetReferences(references);

            ReadPersistence = _dependencyResolver.GetOneRequired<ILoggingPersistence>("read_persistence");
            WritePersistence = _dependencyResolver.GetOneOptional<ILoggingPersistence>("write_persistence");
        }

        public void Configure(ConfigParams config)
        {
            _dependencyResolver.Configure(config);
        }

        public Task<LogMessageV1[]> ReadMessagesAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            return ReadPersistence.GetPageByFilterAsync(correlationId, filter, paging);
        }

        public Task<LogMessageV1[]> ReadErrorsAsync(string correlationId, FilterParams filter, PagingParams paging)
        {
            filter = filter ?? new FilterParams();
            filter.SetAsObject("errors_only", true);

            return ReadPersistence.GetPageByFilterAsync(correlationId, filter, paging);
        }

        public Task WriteMessageAsync(string correlationId, LogMessageV1 message)
        {
            return WritePersistence.CreateAsync(correlationId, message);
        }

        public Task WriteMessagesAsync(string correlationId, LogMessageV1[] messages)
        {
            return Task.Run( ()=> 
            {
                foreach (var message in messages)
                {
                    WritePersistence.CreateAsync(correlationId, message).Wait();
                }
            } );
        }

        public Task ClearAsync(string correlationId)
        {
            return WritePersistence.ClearAsync(correlationId);
        }
    }
}
