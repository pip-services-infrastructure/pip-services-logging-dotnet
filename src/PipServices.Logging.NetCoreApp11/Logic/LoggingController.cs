using System.Linq;
using System.Threading.Tasks;

using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Commons.Refer;
using PipServices.Logging.Persistence;
using PipServices.Logging.Models;
using PipServices.Commons.Commands;
using PipServices.Logging.Logic.Commands;

namespace PipServices.Logging.Logic
{
    public class LoggingController : ILoggingController, IConfigurable
    {
        private readonly DependencyResolver _dependencyResolver;
        private LoggingCommandSet _commandSet;

        private ILoggingPersistence WritePersistence { get; set; }
        private ILoggingPersistence ReadPersistence { get; set; }

        public LoggingController()
        {
            _dependencyResolver = new DependencyResolver();

            _dependencyResolver.Put("read_persistence", new Descriptor("pip-services-logging", "persistence", "*", "*", "*"));
            _dependencyResolver.Put("write_persistence", new Descriptor("pip-services-logging", "persistence", "*", "*", "*"));
        }

        public CommandSet GetCommandSet()
        {
            return _commandSet ?? (_commandSet = new LoggingCommandSet(this));
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
            var tasks = messages.Select(message => WriteMessageAsync(correlationId, message));
            return Task.WhenAll(tasks);
        }

        public Task ClearAsync(string correlationId)
        {
            return WritePersistence.ClearAsync(correlationId);
        }
    }
}
