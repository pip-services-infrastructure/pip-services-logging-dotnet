using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Logging.Persistence;
using PipServices.Commons.Log;
using PipServices.Logging.Clients;
using Xunit;

namespace PipServices.Logging.NetCoreApp11.Test.Clients
{
    public class LoggingDirectClientV1Test : AbstractTest
    {
        private LoggingDirectClientV1 _loggingDirectClient;
        private LoggingController _loggingController;
        private LoggingMemoryPersistence _loggingMemoryPersistence;
        private ConsoleLogger _consoleLogger;

        private TestModel Model { get; set; }

        protected override void Initialize()
        {
            Model = new TestModel();

            _loggingController = new LoggingController();
            _loggingMemoryPersistence = new LoggingMemoryPersistence();
            _consoleLogger = new ConsoleLogger();

            var references = new References();
            references.Put(new Descriptor("pip-services-commons", "logger", "console", "default", "1.0"), _consoleLogger);
            references.Put(new Descriptor("pip-services-logging", "persistence", "memory", "default", "1.0"), _loggingMemoryPersistence);
            references.Put(new Descriptor("pip-services-logging", "controller", "default", "default", "1.0"), _loggingController);

            _loggingController.SetReferences(references);

            _loggingDirectClient = new LoggingDirectClientV1();
            _loggingDirectClient.SetReferences(references);

            _loggingDirectClient.OpenAsync(Model.CorrelationId);
        }

        protected override void Uninitialize()
        {
            _loggingDirectClient.CloseAsync(Model.CorrelationId);
        }

        [Fact]
        public void It_Should_Write_Message_Async_Without_Errors()
        {
            // TODO: Check mock framework!!!
            var result = _loggingDirectClient.WriteMessageAsync(Model.CorrelationId, Model.SampleMessage1);

            Assert.Null(result.Exception);
        }
    }
}